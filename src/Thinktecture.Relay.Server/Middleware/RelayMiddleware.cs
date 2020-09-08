using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc />
	public class RelayMiddleware<TRequest, TResponse> : IMiddleware
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
	{
		private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
		private readonly ILogger<RelayMiddleware<TRequest, TResponse>> _logger;
		private readonly ITenantRepository _tenantRepository;
		private readonly ITenantDispatcher<TRequest> _tenantDispatcher;
		private readonly IResponseCoordinator<TRequest, TResponse> _responseCoordinator;
		private readonly IBodyStore _bodyStore;
		private readonly IRelayTargetResponseWriter<TResponse> _responseWriter;
		private readonly IConnectorTransport<TResponse> _connectorTransport;
		private readonly IRelayContext<TRequest, TResponse> _relayContext;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{TResponse}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="IResponseCoordinator{TRequest,TResponse}"/>.</param>
		/// <param name="connectorTransport">An <see cref="IConnectorTransport{TResponse}"/>.</param>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		public RelayMiddleware(ILogger<RelayMiddleware<TRequest, TResponse>> logger, IRelayClientRequestFactory<TRequest> requestFactory,
			ITenantRepository tenantRepository, ITenantDispatcher<TRequest> tenantDispatcher, IBodyStore bodyStore,
			IRelayTargetResponseWriter<TResponse> responseWriter, IResponseCoordinator<TRequest, TResponse> responseCoordinator,
			IConnectorTransport<TResponse> connectorTransport, IRelayContext<TRequest, TResponse> relayContext)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_connectorTransport = connectorTransport ?? throw new ArgumentNullException(nameof(connectorTransport));
			_relayContext = relayContext ?? throw new ArgumentNullException(nameof(relayContext));
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

			context.Request.EnableBuffering();
			await context.Request.Body.DrainAsync(context.RequestAborted);

			_relayContext.ClientRequest = await _requestFactory.CreateAsync(tenant.Id, context.Request);
			_logger.LogTrace("Parsed request into {@Request}", _relayContext.ClientRequest);

			// TODO call IClientRequestInterceptor

			if (_relayContext.ClientRequest.BodyContent != null && context.Request.Body != _relayContext.ClientRequest.BodyContent)
			{
				// an interceptor changed the body content - need to dispose it properly
				context.Response.RegisterForDisposeAsync(_relayContext.ClientRequest.BodyContent);
			}

			if (_relayContext.TargetResponse == null || _relayContext.ForceConnectorDelivery)
			{
				if (_relayContext.ClientRequest.BodyContent != null)
				{
					await TryStoreBodyContent(context, _relayContext.ClientRequest);
				}

				await _tenantDispatcher.DispatchRequestAsync(_relayContext.ClientRequest);

				_relayContext.TargetResponse = await _responseCoordinator.GetResponseAsync(_relayContext, context.RequestAborted);

				if (_relayContext.ResponseDisposable != null)
				{
					context.Response.RegisterForDisposeAsync(_relayContext.ResponseDisposable);
				}
			}

			_logger.LogTrace("Received response {@Response}", _relayContext.TargetResponse);

			// TODO call ITargetResponseInterceptor

			await _responseWriter.WriteAsync(_relayContext.TargetResponse, context.Response, context.RequestAborted);
		}

		private async Task TryStoreBodyContent(HttpContext context, TRequest request)
		{
			var maximumBodySize = Math.Min(
				_tenantDispatcher.BinarySizeThreshold.GetValueOrDefault(int.MaxValue),
				_connectorTransport.BinarySizeThreshold.GetValueOrDefault(int.MaxValue));

			if (request.BodySize > maximumBodySize)
			{
				_logger.LogInformation(
					"Outsourcing from request {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RequestId}",
					request.BodySize, maximumBodySize, request.RequestId);
				request.BodySize = await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, context.RequestAborted);
				_logger.LogDebug("Outsourced from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);
			}
			else
			{
				request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(context.RequestAborted);
				_logger.LogDebug("Inlined from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);

				context.Response.RegisterForDisposeAsync(request.BodyContent);
			}
		}
	}
}
