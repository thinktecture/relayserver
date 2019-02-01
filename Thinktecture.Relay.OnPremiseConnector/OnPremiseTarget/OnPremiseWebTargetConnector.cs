using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseWebTargetConnector : IOnPremiseTargetConnector
	{
		private readonly ILogger _logger;
		private readonly IOnPremiseWebTargetRequestMessageBuilder _requestMessageBuilder;
		private readonly Uri _baseUri;
		private readonly TimeSpan _requestTimeout;
		private readonly HttpClient _httpClient;

		public OnPremiseWebTargetConnector(Uri baseUri, TimeSpan requestTimeout, ILogger logger, IOnPremiseWebTargetRequestMessageBuilder requestMessageBuilder, IHttpClientFactory httpClientFactory, bool followRedirects)
		{
			if (requestTimeout < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(requestTimeout), "Request timeout cannot be negative.");

			_baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
			_requestTimeout = requestTimeout;
			_logger = logger;
			_requestMessageBuilder = requestMessageBuilder ?? throw new ArgumentNullException(nameof(requestMessageBuilder));

			_httpClient = httpClientFactory.CreateClient(followRedirects ? "FollowRedirectsWebTarget" : "WebTarget");
		}

		public async Task<IOnPremiseTargetResponse> GetResponseFromLocalTargetAsync(string url, IOnPremiseTargetRequest request, string relayedRequestHeader)
		{
			if (url == null)
				throw new ArgumentNullException(nameof(url));
			if (request == null)
				throw new ArgumentNullException(nameof(request));

			_logger?.Verbose("Requesting response from on-premise web target. request-id={RequestId}, url={RequestUrl}, origin-id={OriginId}", request.RequestId, url, request.OriginId);

			var response = new OnPremiseTargetResponse()
			{
				RequestId = request.RequestId,
				OriginId = request.OriginId,
				RequestStarted = DateTime.UtcNow,
			};

			try
			{
				var message = await SendLocalRequestWithTimeoutAsync(url, request, relayedRequestHeader).ConfigureAwait(false);

				response.StatusCode = message.StatusCode;
				response.HttpHeaders = message.Headers.Union(message.Content.Headers).ToDictionary(kvp => kvp.Key, kvp => String.Join(" ", kvp.Value));
				response.Stream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);
				response.HttpResponseMessage = message;
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error requesting response from local target. request-id={RequestId}", request.RequestId);

				response.StatusCode = HttpStatusCode.GatewayTimeout;
				response.HttpHeaders = new Dictionary<string, string> { ["X-TTRELAY-TIMEOUT"] = "On-Premise Target" };
			}

			response.RequestFinished = DateTime.UtcNow;

			_logger?.Verbose("Got web response. request-id={RequestId}, status-code={ResponseStatusCode}", response.RequestId, response.StatusCode);

			return response;
		}

		private async Task<HttpResponseMessage> SendLocalRequestWithTimeoutAsync(String url, IOnPremiseTargetRequest request, String relayedRequestHeader)
		{
			// Only create CTS when really required (i.e. Timeout not Zero or infinite)
			if (_requestTimeout > TimeSpan.Zero && _requestTimeout != TimeSpan.MaxValue)
			{
				using (var cts = new CancellationTokenSource(_requestTimeout))
				{
					return await SendLocalRequestAsync(url, request, relayedRequestHeader, cts.Token).ConfigureAwait(false);
				}
			}

			return await SendLocalRequestAsync(url, request, relayedRequestHeader, CancellationToken.None).ConfigureAwait(false);
		}

		private async Task<HttpResponseMessage> SendLocalRequestAsync(String url, IOnPremiseTargetRequest request, String relayedRequestHeader, CancellationToken token)
		{
			var requestMessage = _requestMessageBuilder.CreateLocalTargetRequestMessage(_baseUri, url, request, relayedRequestHeader);
			return await _httpClient.SendAsync(requestMessage, token).ConfigureAwait(false);
		}
	}
}
