using System;
#if NETSTANDARD2_0
using System.Net;
#endif
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.IdentityModel;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class RelayServerHttpConnection : IRelayServerHttpConnection, IDisposable
	{
		private readonly ILogger _logger;
		private readonly Uri _relayServerUri;
		private readonly TimeSpan _requestTimeout;

		private HttpClient _httpClient;

		public RelayServerHttpConnection(ILogger logger, Uri relayServerUri, TimeSpan requestTimeout)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_relayServerUri = relayServerUri ?? throw new ArgumentNullException(nameof(relayServerUri));
			_requestTimeout = requestTimeout;

#if NETSTANDARD2_0
			ServicePointManager.FindServicePoint(relayServerUri).ConnectionLeaseTimeout = (int)requestTimeout.TotalMilliseconds;

			if (requestTimeout.Milliseconds < ServicePointManager.DnsRefreshTimeout)
			{
				ServicePointManager.DnsRefreshTimeout = (int)requestTimeout.TotalMilliseconds;
			}
#endif

			CreateHttpClient();
		}

		public async Task<HttpResponseMessage> SendToRelayAsync(string relativeUrl, HttpMethod httpMethod, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			if (String.IsNullOrWhiteSpace(relativeUrl))
				throw new ArgumentException("Relative url cannot be null or empty.", nameof(relativeUrl));

			if (!relativeUrl.StartsWith("/"))
				relativeUrl = "/" + relativeUrl;

			var url = new Uri(relativeUrl, UriKind.Relative);

			try
			{
				return await CreateAndSendMessage(httpMethod, url, setHeaders, content, cancellationToken).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				// If it was us that cancelled the request, simply rethrow
				if (cancellationToken.IsCancellationRequested)
					throw;

				// else retry once with new httpClient
				_logger.Error(ex, "Sending http request to RelayServer failed. Replacing HttpClient and re-trying. relay-server={RelayServerUri}", _relayServerUri);

				RecreateHttpClient();
				return await CreateAndSendMessage(httpMethod, url, setHeaders, content, cancellationToken).ConfigureAwait(false);
			}
		}

		private Task<HttpResponseMessage> CreateAndSendMessage(HttpMethod httpMethod, Uri url, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken)
		{
			// We need to create a new HttpRequestMessage even when retrying, as the same message can't be send twice
			var request = new HttpRequestMessage(httpMethod, url);
			setHeaders?.Invoke(request.Headers);
			request.Content = content;

			return _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
		}

		public void SetBearerToken(string accessToken)
		{
			_httpClient.SetBearerToken(accessToken);
		}

		private void RecreateHttpClient()
		{
			var token = _httpClient.GetToken();
			var oldClient = _httpClient;

			CreateHttpClient();
			_httpClient.SetBearerToken(token);

			oldClient?.Dispose();
		}

		private void CreateHttpClient()
		{
			_httpClient = new HttpClient()
			{
				BaseAddress = _relayServerUri,
				Timeout = _requestTimeout,
			};
		}

		public void Dispose()
		{
			Dispose(true);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_httpClient.Dispose();
			}
		}
	}
}
