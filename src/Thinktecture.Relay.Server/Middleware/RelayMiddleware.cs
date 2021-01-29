using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc />
	public class RelayMiddleware<TRequest, TResponse, TAcknowledge> : IMiddleware
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse, new()
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly ILogger<RelayMiddleware<TRequest, TResponse, TAcknowledge>> _logger;
		private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
		private readonly ConnectorRegistry<TRequest> _connectorRegistry;
		private readonly ITenantRepository _tenantRepository;
		private readonly IBodyStore _bodyStore;
		private readonly IRequestCoordinator<TRequest> _requestCoordinator;
		private readonly IRelayTargetResponseWriter<TResponse> _responseWriter;
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IRelayContext<TRequest, TResponse> _relayContext;
		private readonly IEnumerable<IClientRequestInterceptor<TRequest, TResponse>> _clientRequestInterceptors;
		private readonly IEnumerable<ITargetResponseInterceptor<TRequest, TResponse>> _targetResponseInterceptors;
		private readonly IRelayRequestLogger<TRequest, TResponse> _relayRequestLogger;
		private readonly RelayServerOptions _relayServerOptions;
		private readonly int _maximumBodySize;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayMiddleware{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="requestCoordinator">An <see cref="IRequestCoordinator{TRequest}"/>.</param>
		/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{T}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="IResponseCoordinator{T}"/>.</param>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="tenantTransport">An <see cref="ITenantTransport{T}"/>.</param>
		/// <param name="connectorTransportLimit">An <see cref="IConnectorTransportLimit"/>.</param>
		/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="clientRequestInterceptors">An enumeration of <see cref="IClientRequestInterceptor{TRequest,TResponse}"/>.</param>
		/// <param name="targetResponseInterceptors">An enumeration of <see cref="ITargetResponseInterceptor{TRequest,TResponse}"/>.</param>
		/// <param name="relayRequestLogger">An <see cref="IRelayRequestLogger{TRequest,TResponse}"/>.</param>
		public RelayMiddleware(ILogger<RelayMiddleware<TRequest, TResponse, TAcknowledge>> logger,
			IRelayClientRequestFactory<TRequest> requestFactory, ConnectorRegistry<TRequest> connectorRegistry,
			ITenantRepository tenantRepository, IBodyStore bodyStore, IRequestCoordinator<TRequest> requestCoordinator,
			IRelayTargetResponseWriter<TResponse> responseWriter, IResponseCoordinator<TResponse> responseCoordinator,
			IRelayContext<TRequest, TResponse> relayContext, ITenantTransport<TRequest> tenantTransport,
			IConnectorTransportLimit connectorTransportLimit, IOptions<RelayServerOptions> relayServerOptions,
			IEnumerable<IClientRequestInterceptor<TRequest, TResponse>> clientRequestInterceptors,
			IEnumerable<ITargetResponseInterceptor<TRequest, TResponse>> targetResponseInterceptors,
			IRelayRequestLogger<TRequest, TResponse> relayRequestLogger)
		{
			if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));
			if (tenantTransport == null) throw new ArgumentNullException(nameof(tenantTransport));
			if (connectorTransportLimit == null) throw new ArgumentNullException(nameof(connectorTransportLimit));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_requestCoordinator = requestCoordinator ?? throw new ArgumentNullException(nameof(requestCoordinator));
			_responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_relayContext = relayContext ?? throw new ArgumentNullException(nameof(relayContext));
			_clientRequestInterceptors = clientRequestInterceptors ?? throw new ArgumentNullException(nameof(clientRequestInterceptors));
			_targetResponseInterceptors = targetResponseInterceptors ?? throw new ArgumentNullException(nameof(targetResponseInterceptors));
			_relayRequestLogger = relayRequestLogger ?? throw new ArgumentNullException(nameof(relayRequestLogger));

			_relayServerOptions = relayServerOptions.Value;
			_maximumBodySize = Math.Min(tenantTransport.BinarySizeThreshold.GetValueOrDefault(int.MaxValue),
				connectorTransportLimit.BinarySizeThreshold.GetValueOrDefault(int.MaxValue));
		}

		/// <inheritdoc />
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var tenantName = context.Request.Path.Value.Split('/').Skip(1).FirstOrDefault();
			if (string.IsNullOrEmpty(tenantName))
			{
				_logger.LogWarning("Invalid request received {Path}{Query}", context.Request.Path, context.Request.QueryString);
				await next.Invoke(context);
				return;
			}

			var tenant = await _tenantRepository.LoadTenantByNameAsync(tenantName);
			if (tenant == null)
			{
				_logger.LogWarning("Unknown tenant in request received {Path}{Query}", context.Request.Path, context.Request.QueryString);
				await next.Invoke(context);
				return;
			}

			using var cts = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
			if (_relayServerOptions.RequestExpiration != null)
			{
				cts.CancelAfter(_relayServerOptions.RequestExpiration.Value);
			}

			_relayContext.ResponseDisposables.Add(_responseCoordinator.RegisterRequest(_relayContext.RequestId));

			try
			{
				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(cts.Token);

				_relayContext.ClientRequest = await _requestFactory.CreateAsync(tenant.Id, _relayContext.RequestId, context.Request, cts.Token);
				_logger.LogTrace("Parsed request {@Request}", _relayContext.ClientRequest);

				await InterceptClientRequestAsync(cts.Token);

				if (_relayContext.TargetResponse == null || _relayContext.ForceConnectorDelivery)
				{
					await DeliverToConnectorAsync(cts.Token);

					if (_relayContext.TargetResponse == null)
					{
						await WaitForConnectorResponseAsync(cts.Token);
					}
				}

				_logger.LogTrace("Received response for request {RequestId}", _relayContext.RequestId);

				await InterceptTargetResponseAsync(cts.Token);

				await _relayRequestLogger.LogSuccessAsync(_relayContext, CancellationToken.None);
				await _responseWriter.WriteAsync(_relayContext.TargetResponse, context.Response, cts.Token);
			}
			catch (TransportException)
			{
				await _relayRequestLogger.LogFailAsync(_relayContext, CancellationToken.None);
				await WriteErrorResponse(HttpStatusCode.ServiceUnavailable, context.Response, cts.Token);
			}
			catch (OperationCanceledException)
			{
				if (context.RequestAborted.IsCancellationRequested)
				{
					await _relayRequestLogger.LogAbortAsync(_relayContext, CancellationToken.None);
					_logger.LogDebug("Client aborted request {RequestId}", _relayContext.RequestId);
				}
				else
				{
					await _relayRequestLogger.LogExpiredAsync(_relayContext, CancellationToken.None);
					await WriteErrorResponse(HttpStatusCode.RequestTimeout, context.Response, cts.Token);
					_logger.LogWarning("Request {RequestId} expired", _relayContext.RequestId);
				}
			}
			catch (Exception ex)
			{
				await _relayRequestLogger.LogErrorAsync(_relayContext, CancellationToken.None);
				_logger.LogError(ex, "Could not handle request {RequestId}", _relayContext.RequestId);
			}
		}

		private async Task InterceptClientRequestAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Executing client request interceptors for request {RequestId}", _relayContext.RequestId);

			var bodyContent = _relayContext.HttpContext.Request.Body;

			foreach (var interceptor in _clientRequestInterceptors)
			{
				_logger.LogTrace("Executing interceptor {Interceptor} for request {RequestId}", interceptor.GetType().FullName,
					_relayContext.RequestId);
				await interceptor.OnRequestReceivedAsync(_relayContext, cancellationToken);

				if (_relayContext.ClientRequest.BodyContent != null && bodyContent != _relayContext.ClientRequest.BodyContent)
				{
					// an interceptor changed the body content - need to dispose it properly
					_relayContext.ResponseDisposables.Add(_relayContext.ClientRequest.BodyContent);
					bodyContent = _relayContext.ClientRequest.BodyContent;
				}
			}
		}

		private async Task DeliverToConnectorAsync(CancellationToken cancellationToken)
		{
			if (_relayContext.ClientRequest.BodyContent != null &&
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
			var context = await _responseCoordinator.GetResponseAsync(_relayContext.RequestId, cancellationToken);
			if (context == null) return;

			_relayContext.TargetResponse = context.Response;

			if (context.Disposable != null)
			{
				_relayContext.ResponseDisposables.Add(context.Disposable);
			}
		}

		private async Task InterceptTargetResponseAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Executing target response interceptors for request {RequestId}", _relayContext.RequestId);

			foreach (var interceptor in _targetResponseInterceptors)
			{
				_logger.LogTrace("Executing interceptor {Interceptor} for request {RequestId}", interceptor.GetType().FullName,
					_relayContext.RequestId);
				await interceptor.OnResponseReceivedAsync(_relayContext, cancellationToken);
			}
		}

		private async Task<bool> TryInlineBodyContentAsync(TRequest request, CancellationToken cancellationToken)
		{
			if (request.BodyContent == null) return false;

			if (request.BodySize > _maximumBodySize)
			{
				_logger.LogInformation(
					"Outsourcing from request {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RequestId}",
					request.BodySize, _maximumBodySize, request.RequestId);

				await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, cancellationToken);

				request.BodyContent = null;
				_logger.LogDebug("Outsourced from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);

				return false;
			}

			request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(cancellationToken);
			_logger.LogDebug("Inlined from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);

			return true;
		}

		private Task WriteErrorResponse(HttpStatusCode httpStatusCode, HttpResponse response, CancellationToken cancellationToken)
			=> _responseWriter.WriteAsync(_relayContext.ClientRequest.CreateResponse<TResponse>(httpStatusCode), response, cancellationToken);
	}
}
