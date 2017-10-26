using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseWebTargetConnector : IOnPremiseTargetConnector
	{
		private static readonly Dictionary<string, Action<HttpWebRequest, string>> _requestHeaderTransformations;

		static OnPremiseWebTargetConnector()
		{
			_requestHeaderTransformations = new Dictionary<string, Action<HttpWebRequest, string>>()
			{
				["Accept"] = (r, v) => r.Accept = v,
				["Connection"] = (r, v) => r.Connection = v,
				["Content-Type"] = (r, v) => r.ContentType = v,
				["Content-Length"] = (r, v) => r.ContentLength = Int64.Parse(v),
				["Date"] = (r, v) => r.Date = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture),
				["Expect"] = (r, v) => { },
				["Host"] = (r, v) => { },
				["If-Modified-Since"] = (r, v) => r.IfModifiedSince = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture),
				["Proxy-Connection"] = (r, v) => { },
				["Range"] = (r, v) => { },
				["Referer"] = (r, v) => r.Referer = v,
				["Transfer-Encoding"] = (r, v) => r.TransferEncoding = v,
				["User-Agent"] = (r, v) => r.UserAgent = v,
			};
		}

		private readonly Uri _baseUri;
		private readonly int _requestTimeout;
		private readonly bool _ignoreSslErrors;
		private readonly ILogger _logger;

		public OnPremiseWebTargetConnector(Uri baseUri, int requestTimeout, bool ignoreSslErrors, ILogger logger)
		{
			_baseUri = baseUri;
			_requestTimeout = requestTimeout;
			_ignoreSslErrors = ignoreSslErrors;
			_logger = logger;
		}

		public async Task<IOnPremiseTargetResponse> GetResponseFromLocalTargetAsync(string url, IOnPremiseTargetRequest request)
		{
			_logger?.Debug("Requesting response from on-premise web target");
			_logger?.Trace("Requesting response from on-premise web target. request-id={0}, url={1}, origin-id={2}", request.RequestId, url, request.OriginId);

			var response = new OnPremiseTargetResponse()
			{
				RequestId = request.RequestId,
				OriginId = request.OriginId,
				RequestStarted = DateTime.UtcNow,
			};

			var localTargetRequest = await CreateLocalTargetWebRequestAsync(url, request).ConfigureAwait(false);

			try
			{
				// the web response must be disposed later (otherwise the response stream is lost)
				var localTargetResponse = (HttpWebResponse)await localTargetRequest.GetResponseAsync().ConfigureAwait(false);

				response.StatusCode = localTargetResponse.StatusCode;
				response.HttpHeaders = localTargetResponse.Headers.AllKeys.ToDictionary(n => n, n => localTargetResponse.Headers.Get(n), StringComparer.OrdinalIgnoreCase);
				response.Stream = localTargetResponse.GetResponseStream() ?? Stream.Null;
				response.WebResponse = localTargetResponse;
			}
			catch (WebException wex)
			{
				_logger?.Trace("Error requesting response from local target. request-id={0}, exception: {1}", request.RequestId, wex);

				if (wex.Status == WebExceptionStatus.ProtocolError)
				{
					response.WebResponse = (HttpWebResponse)wex.Response;
				}

				_logger?.Warn("Gateway timeout");
				_logger?.Trace("Gateway timeout. request-id={0}", request.RequestId);

				response.StatusCode = HttpStatusCode.GatewayTimeout;
				response.HttpHeaders = new Dictionary<string, string> { ["X-TTRELAY-TIMEOUT"] = "On-Premise Target" };
				response.Stream = Stream.Null;
			}

			response.RequestFinished = DateTime.UtcNow;

			_logger?.Trace("Got response. request-id={0}, status-code={1}", response.RequestId, response.StatusCode);

			return response;
		}

		private async Task<HttpWebRequest> CreateLocalTargetWebRequestAsync(string url, IOnPremiseTargetRequest request)
		{
			_logger?.Trace("Creating web request");

			var localTargetRequest = WebRequest.CreateHttp(String.IsNullOrWhiteSpace(url) ? _baseUri : new Uri(_baseUri, url));
			localTargetRequest.Method = request.HttpMethod;
			localTargetRequest.Timeout = _requestTimeout * 1000;

			if (_ignoreSslErrors)
			{
				localTargetRequest.ServerCertificateValidationCallback += (sender, cert, chain, policy) => true;
			}

			foreach (var httpHeader in request.HttpHeaders)
			{
				_logger?.Trace("   adding header. header={0}, value={1}", httpHeader.Key, httpHeader.Value);

				if (_requestHeaderTransformations.TryGetValue(httpHeader.Key, out var restrictedHeader))
				{
					restrictedHeader(localTargetRequest, httpHeader.Value);
				}
				else
				{
					localTargetRequest.Headers.Add(httpHeader.Key, httpHeader.Value);
				}
			}

			var localTargetStream = Stream.Null;
			if (request.Body?.Length == 0)
			{
				_logger?.Trace("   adding request stream.");

				localTargetStream = await localTargetRequest.GetRequestStreamAsync().ConfigureAwait(false);
				await request.Stream.CopyToAsync(localTargetStream).ConfigureAwait(false);
			}
			else if (request.Body != null)
			{
				_logger?.Trace("   adding request body. length={0}", request.Body.Length);

				localTargetStream = await localTargetRequest.GetRequestStreamAsync().ConfigureAwait(false);
				await localTargetStream.WriteAsync(request.Body, 0, request.Body.Length).ConfigureAwait(false);
			}

			await localTargetStream.FlushAsync().ConfigureAwait(false); // TODO check if needed

			return localTargetRequest;
		}
	}
}
