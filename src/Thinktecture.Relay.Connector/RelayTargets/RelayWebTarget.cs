using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc cref="IRelayTarget{TRequest,TResponse}"/>
	public class RelayWebTarget<TRequest, TResponse> : IRelayTarget<TRequest, TResponse>, IDisposable
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IRelayTargetResponseFactory<TResponse> _responseFactory;
		private readonly HttpClient _client;

		/// <summary>
		/// Initializes a new instance of <see cref="RelayWebTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="responseFactory">The <see cref="IRelayTargetResponseFactory{TResponse}"/> for creating the <typeparamref name="TResponse"/></param>
		/// <param name="clientFactory">The <see cref="IHttpClientFactory"/> for creating the <see cref="HttpClient"/>.</param>
		/// <param name="options">The options.</param>
		public RelayWebTarget(
			IRelayTargetResponseFactory<TResponse> responseFactory,
			IHttpClientFactory clientFactory,
			RelayWebTargetOptions options)
		{
			_responseFactory = responseFactory;

			_client = clientFactory.CreateClient();
			_client.BaseAddress = options.BaseAddress;
			_client.Timeout = options.Timeout;
		}

		/// <inheritdoc />
		public virtual async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			var requestMessage = CreateHttpRequestMessage(request);
			var responseMessage = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
			return await CreateResponseAsync(request, responseMessage);
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
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		protected virtual async Task<TResponse> CreateResponseAsync(TRequest request, HttpResponseMessage responseMessage)
		{
			var response = await _responseFactory.CreateAsync(responseMessage);

			response.RequestId = request.RequestId;
			response.RequestOriginId = request.RequestOriginId;

			// TODO tracing into RequestStart, RequestDuration
			return response;
		}

		/// <summary>
		/// Releases the unmanaged resources used by the <see cref="RelayWebTarget{TRequest,TResponse}"/> and optionally releases the
		/// managed resources.
		/// </summary>
		/// <param name="disposing">true to release both managed and unmanaged resources; false to releases only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_client.Dispose();
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
