using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
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
		private readonly RelayWebTargetOptions _options;
		private readonly ILogger<RelayWebTarget<TRequest, TResponse>> _logger;
		private readonly ITargetResponseFactory<TResponse> _targetResponseFactory;

		/// <summary>
		/// A <see cref="System.Net.Http.HttpClient"/>.
		/// </summary>
		// ReSharper disable once MemberCanBePrivate.Global
		protected readonly HttpClient HttpClient;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="targetResponseFactory">An <see cref="ITargetResponseFactory{TResponse}"/>.</param>
		/// <param name="clientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="baseAddress">The base <see cref="Uri"/> used for the request.</param>
		/// <param name="options">An optional flag build <see cref="RelayWebTargetOptions"/>.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory,
			IHttpClientFactory clientFactory, Uri baseAddress, RelayWebTargetOptions options = RelayWebTargetOptions.None)
			: this(logger, targetResponseFactory, clientFactory)
		{
			_options = options;
			HttpClient.BaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayWebTarget{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="targetResponseFactory">An <see cref="ITargetResponseFactory{TResponse}"/>.</param>
		/// <param name="clientFactory">An <see cref="IHttpClientFactory"/>.</param>
		/// <param name="parameters">The configured parameters.</param>
		public RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory,
			IHttpClientFactory clientFactory, Dictionary<string, string> parameters)
			: this(logger, targetResponseFactory, clientFactory)
		{
			if (!parameters.TryGetValue("Url", out var url) || string.IsNullOrWhiteSpace(url))
			{
				throw new ArgumentException(
					$"The target \"{parameters[Constants.RelayConnectorOptionsTargetId]}\" has no configured base address", nameof(parameters));
			}

			if (!parameters.TryGetValue("Options", out var parameter) || !Enum.TryParse(parameter, true, out _options))
			{
				_options = RelayWebTargetOptions.None;
			}

			HttpClient.BaseAddress = new Uri(url);
		}

		private RelayWebTarget(ILogger<RelayWebTarget<TRequest, TResponse>> logger, ITargetResponseFactory<TResponse> targetResponseFactory,
			IHttpClientFactory clientFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_targetResponseFactory = targetResponseFactory ?? throw new ArgumentNullException(nameof(targetResponseFactory));
			HttpClient = clientFactory?.CreateClient(Constants.RelayWebTargetHttpClientName) ??
				throw new ArgumentNullException(nameof(clientFactory));
		}

		/// <inheritdoc />
		public virtual async Task<TResponse> HandleAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			var start = DateTime.UtcNow;

			_logger.LogTrace("Requesting target for request {RequestId}", request.RequestId);
			var responseMessage = await SendAsync(request, cancellationToken);
			_logger.LogDebug("Requested target for request {RequestId}", request.RequestId);

			var response = await _targetResponseFactory.CreateAsync(request, responseMessage, cancellationToken);

			response.RequestStart = start;
			response.RequestDuration = DateTime.UtcNow - start;

			return response;
		}

		private async Task<HttpResponseMessage> SendAsync(TRequest request, CancellationToken cancellationToken, string url = null)
		{
			while (true)
			{
				var response = await HttpClient.SendAsync(CreateHttpRequestMessage(request, url), HttpCompletionOption.ResponseHeadersRead,
					cancellationToken);

				if ((_options & RelayWebTargetOptions.FollowRedirect) != RelayWebTargetOptions.FollowRedirect)
					return response;

				switch ((int)response.StatusCode)
				{
					// using StatusCodes here, because HttpStatusCode.PermanentRedirect needs netstandard2.1
					case StatusCodes.Status300MultipleChoices:
					case StatusCodes.Status301MovedPermanently:
					case StatusCodes.Status302Found:
					case StatusCodes.Status303SeeOther:
					case StatusCodes.Status307TemporaryRedirect:
					case StatusCodes.Status308PermanentRedirect:
						url = response.Headers.Location?.ToString();
						response.Dispose();

						if (url == null) throw new InvalidOperationException($"Missing \"{HeaderNames.Location}\" header in redirect");

						_logger.LogTrace("Following redirect to location {RedirectLocation} for request {RequestId}", url, request.RequestId);
						break;

					default:
						return response;
				}
			}
		}

		/// <summary>
		/// Transforms the <typeparamref name="TRequest"/> into a <see cref="HttpRequestMessage"/>.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="url">An optional alternative url (e.g. from response's location header).</param>
		/// <returns>A <see cref="HttpRequestMessage"/>.</returns>
		protected virtual HttpRequestMessage CreateHttpRequestMessage(TRequest request, string url = null)
		{
			var requestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), url ?? request.Url);

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
			ITargetResponseFactory<TargetResponse> targetResponseFactory, IHttpClientFactory clientFactory, Uri baseAddress)
			: base(logger, targetResponseFactory, clientFactory, baseAddress)
		{
		}

		/// <inheritdoc />
		public RelayWebTarget(ILogger<RelayWebTarget<ClientRequest, TargetResponse>> logger,
			ITargetResponseFactory<TargetResponse> targetResponseFactory, IHttpClientFactory clientFactory,
			Dictionary<string, string> parameters)
			: base(logger, targetResponseFactory, clientFactory, parameters)
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
