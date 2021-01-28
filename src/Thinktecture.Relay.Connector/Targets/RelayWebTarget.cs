using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <inheritdoc cref="IRelayTarget{TRequest,TResponse}"/>
	public class RelayWebTarget<TRequest, TResponse> : IRelayTarget<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse, new()
	{
		private readonly ILogger<RelayWebTarget<TRequest, TResponse>> _logger;

		/// <summary>
		/// A <see cref="HttpClient"/>.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		protected readonly HttpClient HttpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="baseAddress">The base <see cref="Uri"/> used for the request.</param>
		/// <param name="options">An optional flag build <see cref="RelayWebTargetOptions"/>.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IHttpClientFactory httpClientFactory, Uri baseAddress,
			RelayWebTargetOptions options = RelayWebTargetOptions.None)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			HttpClient = CreateHttpClient(httpClientFactory, options, baseAddress);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="parameters">The configured parameters.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, IHttpClientFactory httpClientFactory,
			Dictionary<string, string> parameters)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			if (!parameters.TryGetValue("Url", out var url) || string.IsNullOrWhiteSpace(url))
			{
				var targetId = parameters[Constants.RelayConnectorOptionsTargetId];
				throw new ArgumentException($"The target \"{targetId}\" has no configured base address", nameof(parameters));
			}

			parameters.TryGetValue("Options", out var parameter);
			Enum.TryParse<RelayWebTargetOptions>(parameter, true, out var options);

			HttpClient = CreateHttpClient(httpClientFactory, options, new Uri(url));
		}

		private HttpClient CreateHttpClient(IHttpClientFactory httpClientFactory, RelayWebTargetOptions options, Uri baseAddress)
		{
			if (httpClientFactory == null) throw new ArgumentNullException(nameof(httpClientFactory));
			if (baseAddress == null) throw new ArgumentNullException(nameof(baseAddress));

			var httpClient = options switch
			{
				RelayWebTargetOptions.FollowRedirect => httpClientFactory.CreateClient(Constants.HttpClientNames.RelayWebTargetFollowRedirect),
				_ => httpClientFactory.CreateClient(Constants.HttpClientNames.RelayWebTargetDefault)
			};

			httpClient.BaseAddress = baseAddress;

			return httpClient;
		}

		/// <inheritdoc />
		public virtual async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			_logger.LogTrace("Requesting target for request {RequestId} at {BaseAddress} for {Url}", request.RequestId, HttpClient.BaseAddress,
				request.Url);

			var requestMessage = CreateHttpRequestMessage(request);
			var responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			_logger.LogDebug("Requested target for request {RequestId} returned {HttpStatusCode}", request.RequestId,
				responseMessage.StatusCode);

			return await responseMessage.CreateResponseAsync<TResponse>(request, cancellationToken);
		}

		/// <summary>
		/// Transforms the <typeparamref name="TRequest"/> into a <see cref="HttpRequestMessage"/>.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="HttpRequestMessage"/>.</returns>
		protected virtual HttpRequestMessage CreateHttpRequestMessage(TRequest request)
		{
			var requestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), request.Url);

			foreach (var (key, value) in request.HttpHeaders)
			{
				if (key == HeaderNames.Host) continue;

				requestMessage.Headers.TryAddWithoutValidation(key, value);
			}

			if (request.BodyContent == null) return requestMessage;

			requestMessage.Content = new StreamContent(request.BodyContent);

			foreach (var (key, value) in request.HttpHeaders)
			{
				requestMessage.Content.Headers.TryAddWithoutValidation(key, value);
			}

			requestMessage.Content.Headers.ContentLength = request.BodySize;

			return requestMessage;
		}

		/// <inheritdoc />
		public void Dispose() => HttpClient.Dispose();
	}

	/// <inheritdoc />
	public class RelayWebTarget : RelayWebTarget<ClientRequest, TargetResponse>
	{
		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger, IHttpClientFactory httpClientFactory,
			Uri baseAddress, RelayWebTargetOptions options = RelayWebTargetOptions.None)
			: base(logger, httpClientFactory, baseAddress, options)
		{
		}

		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger, IHttpClientFactory httpClientFactory,
			Dictionary<string, string> parameters)
			: base(logger, httpClientFactory, parameters)
		{
		}
	}

	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
		/// <summary>
		/// Adds a <see cref="RelayWebTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="builder">The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="baseAddress">An <see cref="Uri"/> containing the base address of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
		/// <returns>The <see cref="IRelayConnectorBuilder{TRequest,TResponse,TAcknowledge}"/>.</returns>
		public static IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> AddWebTarget<TRequest, TResponse, TAcknowledge>(
			this IRelayConnectorBuilder<TRequest, TResponse, TAcknowledge> builder, string id, Uri baseAddress, TimeSpan? timeout = null)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse, new()
			where TAcknowledge : IAcknowledgeRequest
			=> builder.AddTarget<TRequest, TResponse, TAcknowledge, RelayWebTarget<TRequest, TResponse>>(id, timeout, baseAddress);
	}

	/// <summary>
	/// Extension methods for the <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayTargetRegistryExtensions
	{
		/// <summary>
		/// Registers a <see cref="RelayWebTarget{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="relayTargetRegistry">The <see cref="RelayTargetRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="id">The unique id of the target.</param>
		/// <param name="baseAddress">An <see cref="Uri"/> containing the base address of the target.</param>
		/// <param name="timeout">An optional <see cref="TimeSpan"/> when the target times out. The default value is 100 seconds.</param>
		/// <typeparam name="TRequest">The type of request.</typeparam>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		public static void RegisterWebTarget<TRequest, TResponse>(this RelayTargetRegistry<TRequest, TResponse> relayTargetRegistry,
			string id, Uri baseAddress, TimeSpan? timeout = null)
			where TRequest : IClientRequest
			where TResponse : ITargetResponse, new()
			=> relayTargetRegistry.Register<RelayWebTarget<TRequest, TResponse>>(id, timeout, baseAddress);
	}

	/// <summary>
	/// Extension methods for the <see cref="HttpResponseMessage"/>.
	/// </summary>
	public static class HttpResponseMessageExtensions
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="ITargetResponse"/> from <paramref name="request"/> and <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="HttpResponseMessage"/>.</param>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <typeparam name="TResponse">The type of response.</typeparam>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the creation of an instance implementing <see cref="ITargetResponse"/>.</returns>
		public static async Task<TResponse> CreateResponseAsync<TResponse>(this HttpResponseMessage message, IClientRequest request,
			CancellationToken cancellationToken = default)
			where TResponse : ITargetResponse, new()
		{
			var hasBody = (int)message.StatusCode switch
			{
				StatusCodes.Status100Continue => false,
				StatusCodes.Status101SwitchingProtocols => false,
				StatusCodes.Status102Processing => false,
				StatusCodes.Status204NoContent => false,
				StatusCodes.Status304NotModified => false,
				_ => true
			};

			var response = request.CreateResponse<TResponse>();

			response.HttpStatusCode = message.StatusCode;
			response.HttpHeaders = message.Headers.Concat(message.Content.Headers).ToDictionary(h => h.Key, h => h.Value.ToArray());
			response.BodySize = hasBody ? message.Content.Headers.ContentLength : 0;
			response.BodyContent = hasBody ? await message.Content.ReadAsStreamAsync() : null;

			return response;
		}
	}
}
