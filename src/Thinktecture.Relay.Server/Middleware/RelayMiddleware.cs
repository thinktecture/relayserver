using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
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
		private readonly ResponseCoordinator<TRequest, TResponse> _responseCoordinator;
		private readonly IBodyStore _bodyStore;
		private readonly IRelayTargetResponseWriter<TResponse> _responseWriter;
		private readonly IConnectorTransport<TResponse> _connectorTransport;
		private readonly IRelayContext<TRequest, TResponse> _relayContext;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{TResponse}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="ResponseCoordinator{TRequest,TResponse}"/>.</param>
		/// <param name="connectorTransport">An <see cref="IConnectorTransport{TResponse}"/>.</param>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		public RelayMiddleware(IRelayClientRequestFactory<TRequest> requestFactory, ILogger<RelayMiddleware<TRequest, TResponse>> logger,
			ITenantRepository tenantRepository, ITenantDispatcher<TRequest> tenantDispatcher, IBodyStore bodyStore,
			IRelayTargetResponseWriter<TResponse> responseWriter, ResponseCoordinator<TRequest, TResponse> responseCoordinator,
			IConnectorTransport<TResponse> connectorTransport, IRelayContext<TRequest, TResponse> relayContext)
		{
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

			var request = await _requestFactory.CreateAsync(tenant.Id, context.Request);
			_logger.LogTrace("Parsed request into {@ClientRequest}", request);

			_relayContext.ClientRequest = request;

			// TODO call IClientRequestInterceptor

			if (context.Request.Body != request.BodyContent)
			{
				// an interceptor changed the body content - need to dispose it properly
				context.Response.RegisterForDisposeAsync(request.BodyContent);
			}

			if (_relayContext.TargetResponse == null || _relayContext.ForceConnectorDelivery)
			{
				if (request.BodyContent != null)
				{
					await TryStoreBodyContent(context, request);
				}

				await _tenantDispatcher.DispatchRequestAsync(request);

				var response = await _responseCoordinator.GetResponseAsync(_relayContext, context.RequestAborted);

				if (_relayContext.ResponseDisposable != null)
				{
					// context.Response.RegisterForDisposeAsync(_relayContext.ResponseDisposable);
				}

				_relayContext.TargetResponse = response;
			}

			_logger.LogTrace("Received response {@Response}", _relayContext.TargetResponse);

			// TODO call ITargetResponseInterceptor

			await _responseWriter.WriteAsync(_relayContext.TargetResponse, context.Response, context.RequestAborted);

			return;
		}

		private async Task TryStoreBodyContent(HttpContext context, TRequest request)
		{
			var maximumBodySize = Math.Min(
				_tenantDispatcher.BinarySizeThreshold.GetValueOrDefault(int.MaxValue),
				_connectorTransport.BinarySizeThreshold.GetValueOrDefault(int.MaxValue));

			if (request.BodySize > maximumBodySize)
			{
				_logger.LogTrace("Storing too large body with {BodySize} bytes", request.BodySize);
				request.BodySize = await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, context.RequestAborted);
			}
			else
			{
				_logger.LogTrace("Inlining small body with {BodySize} bytes", request.BodySize);
				request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(context.RequestAborted);
				context.Response.RegisterForDisposeAsync(request.BodyContent);
			}
		}
	}
}
