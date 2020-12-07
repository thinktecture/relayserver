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

namespace Thinktecture.Relay.Connector.Targets
{
	/// <inheritdoc cref="IRelayTarget{TRequest,TResponse}"/>
	public class RelayWebTarget<TRequest, TResponse> : IRelayTarget<TRequest, TResponse>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RelayWebTarget<TRequest, TResponse>> _logger;
		private readonly ITargetResponseFactory<TResponse> _targetResponseFactory;

		/// <summary>
		/// A <see cref="HttpClient"/>.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		protected readonly HttpClient HttpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="targetResponseFactory">An <see cref="ITargetResponseFactory{TResponse}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="baseAddress">The base <see cref="Uri"/> used for the request.</param>
		/// <param name="options">An optional flag build <see cref="RelayWebTargetOptions"/>.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory,
			IHttpClientFactory httpClientFactory, Uri baseAddress, RelayWebTargetOptions options = RelayWebTargetOptions.None)
			: this(logger, targetResponseFactory)
			=> HttpClient = CreateHttpClient(httpClientFactory, options, baseAddress);

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

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="targetResponseFactory">An <see cref="ITargetResponseFactory{TResponse}"/>.</param>
		/// <param name="httpClientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="parameters">The configured parameters.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory,
			IHttpClientFactory httpClientFactory, Dictionary<string, string> parameters)
			: this(logger, targetResponseFactory)
		{
			if (!parameters.TryGetValue("Url", out var url) || string.IsNullOrWhiteSpace(url))
			{
				var targetId = parameters[Constants.RelayConnectorOptionsTargetId];
				throw new ArgumentException($"The target \"{targetId}\" has no configured base address", nameof(parameters));
			}

			parameters.TryGetValue("Options", out var parameter);
			Enum.TryParse<RelayWebTargetOptions>(parameter, true, out var options);

			HttpClient = CreateHttpClient(httpClientFactory, options, new Uri(url));
		}

		private RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_targetResponseFactory = targetResponseFactory ?? throw new ArgumentNullException(nameof(targetResponseFactory));
		}

		/// <inheritdoc />
		public virtual async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			var start = DateTime.UtcNow;

			_logger.LogTrace("Requesting target for request {RequestId} at {BaseAddress} for {Url}", request.RequestId, HttpClient.BaseAddress,
				request.Url);

			var requestMessage = CreateHttpRequestMessage(request);
			var responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

			_logger.LogDebug("Requested target for request {RequestId} returned {HttpStatusCode}", request.RequestId,
				responseMessage.StatusCode);

			var response = await _targetResponseFactory.CreateAsync(request, responseMessage, cancellationToken);

			response.RequestStart = start;
			response.RequestDuration = DateTime.UtcNow - start;

#if DEBUG
			response.HttpHeaders["X-RelayServer-Connector-Host"] = new[] { Environment.MachineName };
			response.HttpHeaders["X-RelayServer-Connector-Version"] = new[] { RelayConnector.AssemblyVersion };
#endif

			return response;
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

		/// <inheritdoc />
		public void Dispose() => HttpClient.Dispose();
	}

	/// <inheritdoc />
	public class RelayWebTarget : RelayWebTarget<ClientRequest, TargetResponse>
	{
		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			ITargetResponseFactory<TargetResponse> targetResponseFactory, IHttpClientFactory httpClientFactory, Uri baseAddress,
			RelayWebTargetOptions options = RelayWebTargetOptions.None)
			: base(logger, targetResponseFactory, httpClientFactory, baseAddress, options)
		{
		}

		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			ITargetResponseFactory<TargetResponse> targetResponseFactory, IHttpClientFactory httpClientFactory,
			Dictionary<string, string> parameters)
			: base(logger, targetResponseFactory, httpClientFactory, parameters)
		{
		}
	}

	/// <summary>
	/// Extension methods for the <see cref="IRelayConnectorBuilder{TRequest,TResponse}"/>.
	/// </summary>
	public static class RelayConnectorBuilderExtensions
	{
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
			where TResponse : ITargetResponse
			=> relayTargetRegistry.Register<RelayWebTarget<TRequest, TResponse>>(id, timeout, baseAddress);
	}
}
