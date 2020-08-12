using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core.Features;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc />
	public class RelayMiddleware<TRequest, TResponse> : IMiddleware
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IRelayClientRequestFactory<TRequest> _requestFactory;
		private readonly ILogger<RelayMiddleware<TRequest, TResponse>> _logger;
		private readonly ITenantRepository _tenantRepository;
		private readonly ITenantDispatcher<TRequest> _tenantDispatcher;
		private readonly ResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IBodyStore _bodyStore;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="ResponseCoordinator{TResponse}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		public RelayMiddleware(IRelayClientRequestFactory<TRequest> requestFactory, ILogger<RelayMiddleware<TRequest, TResponse>> logger,
			ITenantRepository tenantRepository, ITenantDispatcher<TRequest> tenantDispatcher,
			ResponseCoordinator<TResponse> responseCoordinator, IBodyStore bodyStore)
		{
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
		}

		/// <inheritdoc />
		public async Task InvokeAsync(HttpContext context, RequestDelegate next)
		{
			var tenantName = context.Request.Path.Value.Split('/').Skip(1).FirstOrDefault();
			if (!string.IsNullOrEmpty(tenantName))
			{
				var tenant = await _tenantRepository.LoadTenantByNameAsync(tenantName);
				if (tenant == null)
				{
					_logger?.LogWarning("Unknown tenant in request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

					await next.Invoke(context);
					return;
				}

				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(context.RequestAborted);

				var request = await _requestFactory.CreateAsync(tenant.Id);
				_logger?.LogTrace("Parsed request into {@ClientRequest}", request);

				// TODO call IClientRequestInterceptor

				await StoreBodyIfNeeded(context, request);

				await _tenantDispatcher.DispatchRequestAsync(request);
				var response = await _responseCoordinator.GetResponseAsync(request.RequestId, context.RequestAborted);

				// TODO call ITargetResponseInterceptor

				// TODO return the real response
				context.Response.ContentType = "application/json";
				await context.Response.WriteAsync(JsonSerializer.Serialize(new { request, response }), context.RequestAborted);

				return;
			}

			_logger?.LogWarning("Invalid request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

			await next.Invoke(context);
		}

		private async Task StoreBodyIfNeeded(HttpContext context, TRequest request)
		{
			if (request.BodyContent != null)
			{
				if (request.BodySize > _tenantDispatcher.BinarySizeThreshold.GetValueOrDefault(int.MaxValue))
				{
					_logger?.LogTrace("Storing too large body {BodySize}", request.BodySize);
					request.BodySize = await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, context.RequestAborted);
				}
				else
				{
					_logger?.LogTrace("Inlining small body {BodySize}", request.BodySize);
					request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(context.RequestAborted);
					context.Response.RegisterForDisposeAsync(request.BodyContent);
				}
			}
		}
	}
}
