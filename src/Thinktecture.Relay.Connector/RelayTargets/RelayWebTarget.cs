using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc cref="IRelayTarget{TRequest,TResponse}"/>
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RelayWebTarget<TRequest, TResponse> : IRelayTarget<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RelayWebTarget<TRequest, TResponse>> _logger;
		private readonly IRelayTargetResponseFactory<TResponse> _responseFactory;

		/// <summary>
		/// A <see cref="System.Net.Http.HttpClient"/>.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		protected readonly HttpClient HttpClient;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayWebTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="responseFactory">An <see cref="IRelayTargetResponseFactory{TResponse}"/>.</param>
		/// <param name="clientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="baseAddress">The base <see cref="Uri"/> used for the request.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IRelayTargetResponseFactory<TResponse> responseFactory,
			IHttpClientFactory clientFactory, Uri baseAddress)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));

			HttpClient = clientFactory?.CreateClient() ?? throw new ArgumentNullException(nameof(clientFactory));
			HttpClient.BaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
			HttpClient.Timeout = Timeout.InfiniteTimeSpan; // will be handled by a cancellation token
		}

		/// <inheritdoc />
		public virtual async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			using var requestMessage = CreateHttpRequestMessage(request);
			var responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			_logger.LogDebug("Requested target for request {RequestId}", request.RequestId);
			return await CreateResponseAsync(request, responseMessage, cancellationToken);

			// TODO error handling when request fails
		}

		/// <summary>
		/// Transforms the <typeparamref name="TRequest"/> into a <see cref="HttpRequestMessage"/>.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="HttpRequestMessage"/>.</returns>
		protected virtual HttpRequestMessage CreateHttpRequestMessage(TRequest request)
		{
			var requestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), request.Url);

			foreach (var header in request.HttpHeaders)
			{
				if (header.Key == HeaderNames.Host)
				{
					continue;
				}

				requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			if (request.BodyContent != null)
			{
				requestMessage.Content = new StreamContent(request.BodyContent);

				foreach (var header in request.HttpHeaders)
				{
					requestMessage.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}

				requestMessage.Content.Headers.ContentLength = request.BodySize;
			}

			return requestMessage;
		}

		/// <summary>
		/// Transforms the <see cref="HttpResponseMessage"/> into a <typeparamref name="TResponse"/>.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="responseMessage">The <see cref="HttpResponseMessage"/> containing the result from the target.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		protected virtual async Task<TResponse> CreateResponseAsync(TRequest request, HttpResponseMessage responseMessage,
			CancellationToken cancellationToken = default)
		{
			var response = await _responseFactory.CreateAsync(request, responseMessage, cancellationToken);

			response.RequestId = request.RequestId;
			response.RequestOriginId = request.RequestOriginId;

			// TODO tracing into RequestStart, RequestDuration

			return response;
		}

		/// <inheritdoc />
		public void Dispose() => HttpClient.Dispose();
	}

	/// <inheritdoc />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RelayWebTarget : RelayWebTarget<ClientRequest, TargetResponse>
	{
		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			IRelayTargetResponseFactory<TargetResponse> responseFactory, IHttpClientFactory clientFactory, Uri baseAddress)
			: base(logger, responseFactory, clientFactory, baseAddress)
		{
		}
	}
}
