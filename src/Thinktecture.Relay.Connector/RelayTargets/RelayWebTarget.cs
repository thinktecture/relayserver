using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.RelayTargets
{
	/// <inheritdoc cref="IRelayTarget{TRequest,TResponse}"/>
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
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="responseFactory">An <see cref="IRelayTargetResponseFactory{TResponse}"/>.</param>
		/// <param name="clientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="baseAddress">The base <see cref="Uri"/> used for the request.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IRelayTargetResponseFactory<TResponse> responseFactory,
			IHttpClientFactory clientFactory, Uri baseAddress)
			: this(logger, responseFactory, clientFactory)
		{
			HttpClient.BaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="responseFactory">An <see cref="IRelayTargetResponseFactory{TResponse}"/>.</param>
		/// <param name="clientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="parameters">The configured parameters.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IRelayTargetResponseFactory<TResponse> responseFactory,
			IHttpClientFactory clientFactory, Dictionary<string, string> parameters)
			: this(logger, responseFactory, clientFactory)
		{
			if (!parameters.TryGetValue("Url", out var url) || string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException(
					$"The target \"{parameters[Constants.RelayConnectorOptionsTargetId]}\" has no configured base address", nameof(parameters));
			}

			HttpClient.BaseAddress = new Uri(url);
		}

		private RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IRelayTargetResponseFactory<TResponse> responseFactory,
			IHttpClientFactory clientFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_responseFactory = responseFactory ?? throw new ArgumentNullException(nameof(responseFactory));

			HttpClient = clientFactory?.CreateClient() ?? throw new ArgumentNullException(nameof(clientFactory));
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

			// TODO tracing into RequestStart, RequestDuration

			return response;
		}

		/// <inheritdoc />
		public void Dispose() => HttpClient.Dispose();
	}

	/// <inheritdoc />
	public class RelayWebTarget : RelayWebTarget<ClientRequest, TargetResponse>
	{
		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			IRelayTargetResponseFactory<TargetResponse> responseFactory, IHttpClientFactory clientFactory, Uri baseAddress)
			: base(logger, responseFactory, clientFactory, baseAddress)
		{
		}

		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			IRelayTargetResponseFactory<TargetResponse> responseFactory, IHttpClientFactory clientFactory,
			Dictionary<string, string> parameters)
			: base(logger, responseFactory, clientFactory, parameters)
		{
		}
	}

	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds a <see cref="RelayWebTarget"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{ClientRequest,TargetResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="baseAddress">An <see cref="Uri"/> containing the base address of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <returns>The <see cref="IRelayConnectorBuilder{ClientRequest,TargetResponse}"/>.</returns>
		public static IRelayConnectorBuilder<ClientRequest, TargetResponse> AddWebTarget(
			this IRelayConnectorBuilder<ClientRequest, TargetResponse> builder, string id, Uri baseAddress, TimeSpan? timeout = null)
			=> builder.AddTarget<ClientRequest, TargetResponse, RelayWebTarget>(id, timeout, baseAddress);

		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="baseAddress">An <see cref="Uri"/> containing the base address of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse> AddWebTarget<TRequest, TResponse>(
			this IRelayConnectorBuilder<TRequest, TResponse> builder, string id, Uri baseAddress, TimeSpan? timeout = null)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse
			=> builder.AddTarget<TRequest, TResponse, RelayWebTarget<TRequest, TResponse>>(id, timeout, baseAddress);
	}
}
