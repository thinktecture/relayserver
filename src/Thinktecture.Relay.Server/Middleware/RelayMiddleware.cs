using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
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
		private readonly ResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IBodyStore _bodyStore;
		private readonly IRelayTargetResponseWriter<TResponse> _responseWriter;
		private readonly IConnectorTransport<TResponse> _connectorTransport;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayMiddleware{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="requestFactory">An <see cref="IRelayClientRequestFactory{TRequest}"/>.</param>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="tenantDispatcher">An <see cref="ITenantDispatcher{TRequest}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="responseWriter">An <see cref="IRelayTargetResponseWriter{TResponse}"/>.</param>
		/// <param name="responseCoordinator">The <see cref="ResponseCoordinator{TResponse}"/>.</param>
		/// <param name="connectorTransport">An <see cref="IConnectorTransport{TResponse}"/>.</param>
		public RelayMiddleware(IRelayClientRequestFactory<TRequest> requestFactory, ILogger<RelayMiddleware<TRequest, TResponse>> logger,
			ITenantRepository tenantRepository, ITenantDispatcher<TRequest> tenantDispatcher, IBodyStore bodyStore,
			IRelayTargetResponseWriter<TResponse> responseWriter, ResponseCoordinator<TResponse> responseCoordinator,
			IConnectorTransport<TResponse> connectorTransport)
		{
			_requestFactory = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_responseWriter = responseWriter ?? throw new ArgumentNullException(nameof(responseWriter));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_connectorTransport = connectorTransport ?? throw new ArgumentNullException(nameof(connectorTransport));
		}

		private class FakeResponseGenerator
		{
			public async Task<TResponse> GenerateAsync(TRequest request)
			{
				using var httpClient = new HttpClient();

				var requestMessage = new HttpRequestMessage
				{
					Method = HttpMethod.Get,
					RequestUri = new Uri("https://www.google.com/")
				};

				foreach (var (name, values) in request.HttpHeaders)
				{
					if (name == HeaderNames.Host)
					{
						continue;
					}

					requestMessage.Headers.TryAddWithoutValidation(name, values);
				}

				var responseMessage = await httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);

				var response = new TResponse
				{
					HttpStatusCode = responseMessage.StatusCode,
					HttpHeaders = responseMessage.Headers
						.Concat(responseMessage.Content.Headers)
						.ToDictionary(h => h.Key, h => h.Value.ToArray()),
					BodySize = responseMessage.Content.Headers.ContentLength,
				};

				await using var stream = await responseMessage.Content.ReadAsStreamAsync();
				response.BodyContent = await stream.CopyToMemoryStreamAsync();

				return response;
			}
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
					_logger.LogWarning("Unknown tenant in request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

					await next.Invoke(context);
					return;
				}

				context.Request.EnableBuffering();
				await context.Request.Body.DrainAsync(context.RequestAborted);

				var request = await _requestFactory.CreateAsync(tenant.Id, context.Request);
				_logger.LogTrace("Parsed request into {@ClientRequest}", request);

				// TODO call IClientRequestInterceptor

				if (request.BodyContent != null)
				{
					if (context.Request.Body != request.BodyContent)
					{
						// an interceptor changed the body content - need to dispose it properly
						context.Response.RegisterForDisposeAsync(request.BodyContent);
					}

					await TryStoreBodyContent(context, request);
				}

				await _tenantDispatcher.DispatchRequestAsync(request);

				var response = await _responseCoordinator.GetResponseAsync(request.RequestId, context.RequestAborted);
				// var response = await new FakeResponseGenerator().GenerateAsync(request); // TODO remove this when connector is available

				_logger.LogTrace("Received response {@TargetResponse}", response);

				// TODO call ITargetResponseInterceptor

				await _responseWriter.WriteAsync(response, context.Response, context.RequestAborted);

				return;
			}

			_logger.LogWarning("Invalid request received {Path}{Query}", context.Request.Path, context.Request.QueryString);

			await next.Invoke(context);
		}

		private async Task TryStoreBodyContent(HttpContext context, TRequest request)
		{
			var maximumBodySize = Math.Min(
				_tenantDispatcher.BinarySizeThreshold.GetValueOrDefault(int.MaxValue),
				_connectorTransport.BinarySizeThreshold.GetValueOrDefault(int.MaxValue));

			if (request.BodySize > maximumBodySize)
			{
				_logger.LogTrace("Storing too large body {BodySize}", request.BodySize);
				request.BodySize = await _bodyStore.StoreRequestBodyAsync(request.RequestId, request.BodyContent, context.RequestAborted);
			}
			else
			{
				_logger.LogTrace("Inlining small body {BodySize}", request.BodySize);
				request.BodyContent = await request.BodyContent.CopyToMemoryStreamAsync(context.RequestAborted);
				context.Response.RegisterForDisposeAsync(request.BodyContent);
			}
		}
	}
}
