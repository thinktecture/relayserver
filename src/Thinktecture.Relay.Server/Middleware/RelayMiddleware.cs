using System;
using System.IO;
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
		where TResponse : class, ITargetResponse, new()
	{
		private readonly ILogger<RelayMiddleware<TRequest, TResponse>> _logger;
		private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
		private readonly ITenantRepository _tenantRepository;
		private readonly IBodyStore _bodyStore;
		private readonly IRequestCoordinator<TRequest> _requestCoordinator;
		private readonly IRelayTargetResponseWriter<TResponse> _responseWriter;
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IRelayContext<TRequest, TResponse> _relayContext;
		private readonly int _maximumBodySize;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="requestCoordinator">An <see cref="IRequestCoordinator{TRequest}"/>.</param>
		/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{TResponse}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="IResponseCoordinator{TResponse}"/>.</param>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="connectorTransport">An <see cref="IConnectorTransport{TResponse}"/>.</param>
		public RelayMiddleware(ILogger<RelayMiddleware<TRequest, TResponse>> logger, IRelayClientRequestFactory<TRequest> requestFactory,
			ITenantRepository tenantRepository, IBodyStore bodyStore, IRequestCoordinator<TRequest> requestCoordinator,
			IRelayTargetResponseWriter<TResponse> responseWriter, IResponseCoordinator<TResponse> responseCoordinator,
			IRelayContext<TRequest, TResponse> relayContext, ITenantDispatcher<TRequest> tenantDispatcher,
			IConnectorTransport<TResponse> connectorTransport)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_requestCoordinator = requestCoordinator ?? throw new ArgumentNullException(nameof(requestCoordinator));
			_responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_relayContext = relayContext ?? throw new ArgumentNullException(nameof(relayContext));

			if (tenantDispatcher == null) throw new ArgumentNullException(nameof(tenantDispatcher));
			if (connectorTransport == null) throw new ArgumentNullException(nameof(connectorTransport));

			_maximumBodySize = Math.Min(tenantDispatcher.BinarySizeThreshold.GetValueOrDefault(),
				connectorTransport.BinarySizeThreshold.GetValueOrDefault());
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

			try
			{
				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(context.RequestAborted);

				_relayContext.ClientRequest = await _requestFactory.CreateAsync(tenant.Id, _relayContext.RequestId, context.Request);
				_logger.LogTrace("Parsed request {@Request}", _relayContext.ClientRequest);

				// TODO call IClientRequestInterceptor

				if (_relayContext.ClientRequest.BodyContent != null && context.Request.Body != _relayContext.ClientRequest.BodyContent)
				{
					// an interceptor changed the body content - need to dispose it properly
					_relayContext.ResponseDisposables.Add(_relayContext.ClientRequest.BodyContent);
				}

				if (_relayContext.TargetResponse == null || _relayContext.ForceConnectorDelivery)
				{
					if (_relayContext.ClientRequest.BodyContent != null && await TryInlineBodyContent(context, _relayContext.ClientRequest))
					{
						_relayContext.ResponseDisposables.Add(_relayContext.ClientRequest.BodyContent);
					}

					await _requestCoordinator.DeliverRequestAsync(_relayContext.ClientRequest, context.RequestAborted);

					var (response, disposable) = await _responseCoordinator.GetResponseAsync(_relayContext.RequestId, context.RequestAborted);
					_relayContext.TargetResponse = response;

					if (disposable != null)
					{
						_relayContext.ResponseDisposables.Add(disposable);
					}
				}

				_logger.LogTrace("Received response {@Response}", _relayContext.TargetResponse);

				if (_relayContext.TargetResponse.RequestFailed)
				{
					_logger.LogWarning("The request {RequestId} failed on the connector side with {HttpStatusCode}", _relayContext.RequestId,
						_relayContext.TargetResponse.HttpStatusCode);
				}

				// TODO call ITargetResponseInterceptor

				await _responseWriter.WriteAsync(_relayContext.TargetResponse, context.Response, context.RequestAborted);
			}
			catch (TaskCanceledException)
			{
				_logger.LogDebug("Client aborted request {RequestId}", _relayContext.RequestId);
			}
		}

		private async Task<bool> TryInlineBodyContent(HttpContext context, TRequest request)
		{
			if (request.BodySize > _maximumBodySize)
			{
				_logger.LogInformation(
					"Outsourcing from request {BodySize} bytes because of a maximum of {BinarySizeThreshold} for request {RequestId}",
					request.BodySize, _maximumBodySize, request.RequestId);
				request.BodySize = await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, context.RequestAborted);
				request.BodyContent = null;
				_logger.LogDebug("Outsourced from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);
				return false;
			}

			request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(context.RequestAborted);
			_logger.LogDebug("Inlined from request {BodySize} bytes for request {RequestId}", request.BodySize, request.RequestId);
			return true;
		}
	}
}
