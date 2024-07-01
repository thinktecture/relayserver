using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Extensions;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware;

internal static class RelayMiddleware
{
	public static readonly ConcurrentDictionary<string, object> CacheLocks = new ConcurrentDictionary<string, object>();
}

/// <inheritdoc />
public partial class RelayMiddleware<TRequest, TResponse, TAcknowledge> : IMiddleware
	where TRequest : class, IClientRequest
	where TResponse : class, ITargetResponse, new()
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IBodyStore _bodyStore;
	private readonly IEnumerable<IClientRequestInterceptor<TRequest, TResponse>> _clientRequestInterceptors;
	private readonly ConnectorRegistry<TRequest> _connectorRegistry;
	private readonly ILogger _logger;

	private readonly int _maximumBodySize;
	private readonly IRelayContext<TRequest, TResponse> _relayContext;
	private readonly IRelayRequestLogger<TRequest, TResponse> _relayRequestLogger;
	private readonly IDistributedCache _cache;
	private readonly RelayServerOptions _relayServerOptions;
	private readonly JwtBearerOptions? _jwtBearerOptions;
	private readonly IRequestCoordinator<TRequest> _requestCoordinator;
	private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
	private readonly IResponseCoordinator<TResponse> _responseCoordinator;
	private readonly IRelayTargetResponseWriter<TRequest, TResponse> _responseWriter;
	private readonly IEnumerable<ITargetResponseInterceptor<TRequest, TResponse>> _targetResponseInterceptors;
	private readonly ITenantService _tenantService;

	/// <summary>
	/// Initializes a new instance of the <see cref="RelayMiddleware{TRequest,TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
	/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
	/// <param name="tenantService">An <see cref="ITenantService"/>.</param>
	/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
	/// <param name="requestCoordinator">An <see cref="IRequestCoordinator{TRequest}"/>.</param>
	/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{TRequest,TResponse}"/>.</param>
	/// <param name="responseCoordinator">The <see cref="IResponseCoordinator{T}"/>.</param>
	/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
	/// <param name="tenantTransport">An <see cref="ITenantTransport{T}"/>.</param>
	/// <param name="connectorTransportLimit">An <see cref="IConnectorTransportLimit"/>.</param>
	/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="clientRequestInterceptors">An enumeration of <see cref="IClientRequestInterceptor{TRequest,TResponse}"/>.</param>
	/// <param name="targetResponseInterceptors">
	/// An enumeration of <see cref="ITargetResponseInterceptor{TRequest,TResponse}"/>
	/// .
	/// </param>
	/// <param name="relayRequestLogger">An <see cref="IRelayRequestLogger{TRequest,TResponse}"/>.</param>
	/// <param name="cache">An implementation of <see cref="IDistributedCache"/></param>
	/// <param name="jwtBearerOptions">An <see cref="IOptionsSnapshot{TOptions}"/>.</param>
	public RelayMiddleware(ILogger<RelayMiddleware<TRequest, TResponse, TAcknowledge>> logger,
		IRelayClientRequestFactory<TRequest> requestFactory, ConnectorRegistry<TRequest> connectorRegistry,
		ITenantService tenantService, IBodyStore bodyStore, IRequestCoordinator<TRequest> requestCoordinator,
		IRelayTargetResponseWriter<TRequest, TResponse> responseWriter,
		IResponseCoordinator<TResponse> responseCoordinator, IRelayContext<TRequest, TResponse> relayContext,
		ITenantTransport<TRequest> tenantTransport, IConnectorTransportLimit connectorTransportLimit,
		IOptions<RelayServerOptions> relayServerOptions,
		IEnumerable<IClientRequestInterceptor<TRequest, TResponse>> clientRequestInterceptors,
		IEnumerable<ITargetResponseInterceptor<TRequest, TResponse>> targetResponseInterceptors,
		IRelayRequestLogger<TRequest, TResponse> relayRequestLogger, IDistributedCache cache,
		IOptionsSnapshot<JwtBearerOptions>? jwtBearerOptions)
	{
		if (relayServerOptions is null) throw new ArgumentNullException(nameof(relayServerOptions));
		if (tenantTransport is null) throw new ArgumentNullException(nameof(tenantTransport));
		if (connectorTransportLimit is null) throw new ArgumentNullException(nameof(connectorTransportLimit));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
		_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
		_tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
		_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
		_requestCoordinator = requestCoordinator ?? throw new ArgumentNullException(nameof(requestCoordinator));
		_responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
		_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
		_relayContext = relayContext ?? throw new ArgumentNullException(nameof(relayContext));
		_clientRequestInterceptors = clientRequestInterceptors ??
			throw new ArgumentNullException(nameof(clientRequestInterceptors));
		_targetResponseInterceptors = targetResponseInterceptors ??
			throw new ArgumentNullException(nameof(targetResponseInterceptors));
		_relayRequestLogger = relayRequestLogger ?? throw new ArgumentNullException(nameof(relayRequestLogger));
		_cache = cache ?? throw new ArgumentNullException(nameof(cache));

		_relayServerOptions = relayServerOptions.Value;
		_jwtBearerOptions = jwtBearerOptions?.Get(Constants.DefaultAuthenticationScheme);

		_maximumBodySize = Math.Min(tenantTransport.BinarySizeThreshold.GetValueOrDefault(int.MaxValue),
			connectorTransportLimit.BinarySizeThreshold.GetValueOrDefault(int.MaxValue));
	}

	/// <inheritdoc />
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		var (_, tenantName, _, _) = context.Request.GetRelayRequest();

		using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
		if (_relayServerOptions.RequestExpiration is not null)
		{
			cts.CancelAfter(_relayServerOptions.RequestExpiration.Value);
		}

		try
		{
			if (string.IsNullOrEmpty(tenantName))
			{
				Log.InvalidRequest(_logger, context.Request.Path, context.Request.QueryString);
				await next.Invoke(context);
				return;
			}

			var tenantState = await LoadTenantStateByNameAsync(tenantName, CancellationToken.None);
			if (tenantState.Unknown)
			{
				Log.UnknownTenant(_logger, tenantName, context.Request.Path, context.Request.QueryString);
				await next.Invoke(context);
				return;
			}

			if (tenantState.RequireAuthentication && !IsAuthenticated(context))
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}

			// ensure the correct casing (ignore it from url starting from here)
			tenantName = tenantState.TenantName;

			if (_relayServerOptions.RequireActiveConnection && !tenantState.HasActiveConnections)
			{
				Log.NoActiveConnection(_logger, tenantName);
				context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
				return;
			}

			context.Request.EnableBuffering();
			await context.Request.Body.DrainAsync(cts.Token);

			_relayContext.ClientRequest =
				await _requestFactory.CreateAsync(tenantName, _relayContext.RequestId, context.Request, cts.Token);

			Log.RequestParsed(_logger, _relayContext.ClientRequest);

			await InterceptClientRequestAsync(cts.Token);

			if (_relayContext.TargetResponse is not null)
			{
				// if we already have a response we can discard any connector response
				_relayContext.ClientRequest.DiscardConnectorResponse = true;
			}

			if (_relayContext.ClientRequest.DiscardConnectorResponse)
			{
				Log.DiscardConnectorResponse(_logger, _relayContext.RequestId);
			}
			else
			{
				_relayContext.ResponseDisposables.Add(_responseCoordinator.RegisterRequest(_relayContext.RequestId));
			}

			if (_relayContext.TargetResponse is null || _relayContext.ForceConnectorDelivery)
			{
				if (tenantState.HasRequestsLimit && _relayContext.ClientRequest.AcknowledgeMode != AcknowledgeMode.Manual)
				{
					_relayContext.ClientRequest.AcknowledgeMode = AcknowledgeMode.ConnectorFinished;
					Log.AcknowledgeModeChange(_logger, _relayContext.RequestId);
				}

				await DeliverToConnectorAsync(cts.Token);

				if (!_relayContext.ClientRequest.DiscardConnectorResponse)
				{
					await WaitForConnectorResponseAsync(cts.Token);
					Log.ResponseReceived(_logger, _relayContext.RequestId);
				}
			}

			if (_relayContext.ClientRequest.DiscardConnectorResponse && _relayContext.TargetResponse is null)
			{
				_relayContext.TargetResponse =
					_relayContext.ClientRequest.CreateResponse<TResponse>(HttpStatusCode.Accepted);
			}

			await InterceptTargetResponseAsync(cts.Token);

			await _responseWriter.WriteAsync(_relayContext.ClientRequest, _relayContext.TargetResponse, context.Response,
				cts.Token);
			await _relayRequestLogger.LogSuccessAsync(_relayContext,
				_relayContext.ClientRequest.BodySize.GetValueOrDefault(), context.Request, _relayContext.TargetResponse);
		}
		catch (TransportException)
		{
			await _relayRequestLogger.LogFailAsync(_relayContext, _relayContext.ClientRequest.BodySize.GetValueOrDefault(),
				context.Request);
			await WriteErrorResponse(HttpStatusCode.ServiceUnavailable, context.Response, cts.Token);
		}
		catch (OperationCanceledException)
		{
			// if the exception is raised too early we don't have a client request yet
			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
			var bodySize = _relayContext.ClientRequest?.BodySize ?? context.Request.Body.Length;

			if (context.RequestAborted.IsCancellationRequested)
			{
				await _relayRequestLogger.LogAbortAsync(_relayContext, bodySize, context.Request);
				Log.ClientAborted(_logger, _relayContext.RequestId);
			}
			else
			{
				await _relayRequestLogger.LogExpiredAsync(_relayContext, bodySize, context.Request);
				await WriteErrorResponse(HttpStatusCode.RequestTimeout, context.Response, cts.Token);
				Log.RequestExpired(_logger, _relayContext.RequestId);
			}
		}
		catch (Exception ex)
		{
			await WriteErrorResponse(HttpStatusCode.InternalServerError, context.Response, cts.Token);

			// if the exception is raised too early we don't have a client request yet
			// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
			var bodySize = _relayContext.ClientRequest?.BodySize ?? context.Request.Body.Length;
			await _relayRequestLogger.LogErrorAsync(_relayContext, bodySize, context.Request);
			Log.ErrorHandlingRequest(_logger, ex, _relayContext.RequestId);
		}
	}

	private bool IsAuthenticated(HttpContext context)
	{
		if (_jwtBearerOptions?.Authority is null || _jwtBearerOptions?.Audience is null) return false;

		if (context.User.Identity is not ClaimsIdentity { IsAuthenticated: true } identity) return false;

		return identity.HasClaim(JwtRegisteredClaimNames.Iss, _jwtBearerOptions.Authority) &&
			identity.HasClaim(JwtRegisteredClaimNames.Aud, _jwtBearerOptions.Audience);
	}

	private async Task InterceptClientRequestAsync(CancellationToken cancellationToken)
	{
		Log.ExecutingRequestInterceptors(_logger, _relayContext.RequestId);

		var clientRequest = _relayContext.ClientRequest;
		var bodyContent = _relayContext.HttpContext.Request.Body;

		foreach (var interceptor in _clientRequestInterceptors)
		{
			Log.ExecutingRequestInterceptor(_logger, interceptor.GetType().FullName, _relayContext.RequestId);
			await interceptor.OnRequestReceivedAsync(_relayContext, cancellationToken);

			if (clientRequest.BodyContent != bodyContent)
			{
				// an interceptor changed the body content - need to dispose it properly (but not the request body)
				if (bodyContent is not null && bodyContent != _relayContext.HttpContext.Request.Body)
				{
					_relayContext.ResponseDisposables.Add(bodyContent);
				}
				bodyContent = clientRequest.BodyContent;
			}
		}

		// try to rewind, in order to start sending from the start
		bodyContent?.TryRewind();

		// if possible, try to update body size (interceptor should have done that already, just to be sure)
		if (bodyContent?.CanSeek == true)
		{
			clientRequest.BodySize = bodyContent.Length;
		}
		else if (bodyContent is null)
		{
			clientRequest.BodySize = 0;
		}
	}

	private async Task DeliverToConnectorAsync(CancellationToken cancellationToken)
	{
		Log.DeliveringRequest(_logger, _relayContext.RequestId);

		if (_relayContext.ClientRequest.BodyContent is not null &&
			await TryInlineBodyContentAsync(_relayContext.ClientRequest, cancellationToken))
		{
			_relayContext.ResponseDisposables.Add(_relayContext.ClientRequest.BodyContent);
		}

		if (_relayServerOptions.EnableConnectorTransportShortcut &&
			await _connectorRegistry.TryDeliverRequestAsync(_relayContext.ClientRequest, cancellationToken)) return;

		await _requestCoordinator.ProcessRequestAsync(_relayContext.ClientRequest, cancellationToken);
	}

	private async Task WaitForConnectorResponseAsync(CancellationToken cancellationToken)
	{
		Log.WaitForResponse(_logger, _relayContext.RequestId);

		var context = await _responseCoordinator.GetResponseAsync(_relayContext.RequestId, cancellationToken);
		if (context is null) return;

		_relayContext.TargetResponse = context.Response;

		if (context.Disposable is not null)
		{
			_relayContext.ResponseDisposables.Add(context.Disposable);
		}
	}

	private async Task InterceptTargetResponseAsync(CancellationToken cancellationToken)
	{
		Log.ExecutingResponseInterceptors(_logger, _relayContext.RequestId);

		var bodyContent = _relayContext.TargetResponse?.BodyContent;

		foreach (var interceptor in _targetResponseInterceptors)
		{
			Log.ExecutingResponseInterceptor(_logger, interceptor.GetType().FullName, _relayContext.RequestId);
			await interceptor.OnResponseReceivedAsync(_relayContext, cancellationToken);

			if (_relayContext.TargetResponse?.BodyContent == bodyContent) continue;

			// an interceptor changed the body content - need to dispose it properly
			if (bodyContent is not null)
			{
				_relayContext.ResponseDisposables.Add(bodyContent);
			}
			bodyContent = _relayContext.TargetResponse?.BodyContent;
		}

		// try to rewind, in order to start sending from the start
		bodyContent?.TryRewind();

		if (_relayContext.TargetResponse is not null)
		{
			// if possible, try to update body size (interceptor should have done that already, just to be sure)
			if (bodyContent?.CanSeek == true)
			{
				_relayContext.TargetResponse.BodySize = bodyContent.Length;
			}
			else if (bodyContent is null)
			{
				_relayContext.TargetResponse.BodySize = 0;
			}
		}
	}

	private async Task<bool> TryInlineBodyContentAsync(TRequest request, CancellationToken cancellationToken)
	{
		if (request.BodyContent is null) return false;

		if (request.BodySize > _maximumBodySize)
		{
			Log.OutsourcingRequestBody(_logger, request.BodySize, request.OriginalBodySize, _maximumBodySize,
				request.RequestId);
			await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, cancellationToken);

			request.BodyContent = null;
			Log.OutsourcedRequestBody(_logger, request.BodySize, request.OriginalBodySize, request.RequestId);

			return false;
		}

		request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
		Log.InlinedRequestBody(_logger, request.BodySize, request.OriginalBodySize, request.RequestId);

		return true;
	}

	private Task WriteErrorResponse(HttpStatusCode httpStatusCode, HttpResponse response,
		CancellationToken cancellationToken)
	{
		var targetResponse = _relayContext.ClientRequest.CreateResponse<TResponse>(httpStatusCode);
		return _responseWriter.WriteAsync(_relayContext.ClientRequest, targetResponse, response, cancellationToken);
	}

	private async Task<TenantState> LoadTenantStateByNameAsync(string name, CancellationToken cancellationToken)
	{
		var cacheKey = $"tenant_state_{_tenantService.NormalizeName(name)}";
		var cachedData = await _cache.GetAsync(cacheKey, cancellationToken);
		if (cachedData is not null) return TenantState.FromSpan(cachedData);

		lock (RelayMiddleware.CacheLocks.GetOrAdd(cacheKey, () => new object()))
		{
			cachedData = _cache.Get(cacheKey);
			if (cachedData is not null) return TenantState.FromSpan(cachedData);

			try
			{
				var tenant = _tenantService.LoadTenantWithConnections(name);
				var state = TenantState.FromTenant(tenant);

				var cacheEntryOptions = new DistributedCacheEntryOptions()
					.SetAbsoluteExpiration(_relayServerOptions.TenantInfoCacheDuration);
				_cache.Set(cacheKey, TenantState.AsSpan(state).ToArray(), cacheEntryOptions);

				return state;
			}
			catch
			{
				return TenantState.FromTenant(null);
			}
		}
	}

	private record TenantState
	{
		public bool Unknown { get; private init; }

		public bool HasActiveConnections { get; private init; }

		public bool RequireAuthentication { get; private init; }

		public bool HasRequestsLimit { get; private init; }

		public string TenantName { get; private init; } = string.Empty;

		public static TenantState FromTenant(Tenant? tenant)
		{
			if (tenant is null) return new TenantState() { Unknown = true };

			return new TenantState
			{
				TenantName = tenant.Name,
				HasActiveConnections = tenant.Connections?.Any(c => c.DisconnectTime is null) ?? false,
				RequireAuthentication = tenant.RequireAuthentication,
				HasRequestsLimit = tenant.MaximumConcurrentConnectorRequests > 0,
			};
		}

		public static TenantState FromSpan(Span<byte> span)
			=> new TenantState()
			{
				Unknown = span[0] == byte.MaxValue,
				HasActiveConnections = span[1] == byte.MaxValue,
				RequireAuthentication = span[2] == byte.MaxValue,
				HasRequestsLimit = span[3] == byte.MaxValue,
				TenantName = Encoding.Unicode.GetString(span[4..]),
			};

		public static Span<byte> AsSpan(TenantState tenantState)
		{
			var tenantName = Encoding.Unicode.GetBytes(tenantState.TenantName);

			var buffer = new byte[4 + tenantName.Length];
			buffer[0] = tenantState.Unknown ? byte.MaxValue : byte.MinValue;
			buffer[1] = tenantState.HasActiveConnections ? byte.MaxValue : byte.MinValue;
			buffer[2] = tenantState.RequireAuthentication ? byte.MaxValue : byte.MinValue;
			buffer[3] = tenantState.HasRequestsLimit ? byte.MaxValue : byte.MinValue;
			tenantName.CopyTo(buffer, 4);

			return buffer;
		}
	}
}
