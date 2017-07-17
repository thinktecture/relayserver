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
		private static readonly Action<HttpWebRequest, string> _nullAction = (r, v) => { };

		static OnPremiseWebTargetConnector()
		{
			_requestHeaderTransformations = new Dictionary<string, Action<HttpWebRequest, string>>()
			{
				{"Accept", (r, v) => r.Accept = v},
				{"Connection", (r, v) => r.Connection = v},
				{"Content-Type", (r, v) => r.ContentType = v},
				{"Content-Length", (r, v) => r.ContentLength = Int64.Parse(v)},
				{"Date", (r, v) => r.Date = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture)},
				{"Expect", _nullAction},
				{"Host", _nullAction},
				{"If-Modified-Since", (r, v) => r.IfModifiedSince = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture)},
				{"Proxy-Connection", _nullAction},
				{"Range", _nullAction},
				{"Referer", (r, v) => r.Referer = v},
				{"Transfer-Encoding", (r, v) => r.TransferEncoding = v},
				{"User-Agent", (r, v) => r.UserAgent = v}
			};
		}

		private readonly Uri _baseUri;
		private readonly int _requestTimeout;
		private readonly ILogger _logger;

		public OnPremiseWebTargetConnector(Uri baseUri, int requestTimeout, ILogger logger)
		{
			_baseUri = baseUri;
			_requestTimeout = requestTimeout;
			_logger = logger;
		}

		public async Task<IOnPremiseTargetResponse> GetResponseAsync(string url, IOnPremiseTargetRequest request)
		{
			_logger.Debug("Requesting response from on-premise web target");
			_logger.Trace("Requesting response from on-premise web target. request-id={0}, url={1}, origin-id={2}", request.RequestId, url, request.OriginId);

			var response = new OnPremiseTargetResponse()
			{
				RequestId = request.RequestId,
				OriginId = request.OriginId,
				RequestStarted = DateTime.UtcNow
			};

			var webRequest = await CreateOnPremiseTargetWebRequestAsync(url, request);

			HttpWebResponse webResponse = null;
			try
			{
				try
				{
					webResponse = (HttpWebResponse) await webRequest.GetResponseAsync();
				}
				catch (WebException wex)
				{
					_logger.Trace("Error requesting response. request-id={0}, exception: {1}", request.RequestId, wex);

					if (wex.Status == WebExceptionStatus.ProtocolError)
					{
						webResponse = (HttpWebResponse) wex.Response;
					}
				}

				if (webResponse == null)
				{
					_logger.Warn("Gateway timeout!");
					_logger.Trace("Gateway timeout. request-id={0}", request.RequestId);

					response.StatusCode = HttpStatusCode.GatewayTimeout;
					response.HttpHeaders = new Dictionary<string, string>()
					{
						{"X-TTRELAY-TIMEOUT", "On-Premise Target"}
					};
				}
				else
				{
					response.StatusCode = webResponse.StatusCode;

					using (var stream = new MemoryStream())
					{
						response.HttpHeaders = webResponse.Headers.AllKeys.ToDictionary(n => n, n => webResponse.Headers.Get(n), StringComparer.OrdinalIgnoreCase);

						using (var responseStream = webResponse.GetResponseStream() ?? Stream.Null)
						{
							await responseStream.CopyToAsync(stream);
						}

						response.Body = stream.ToArray();
					}
				}

				response.RequestFinished = DateTime.UtcNow;

				_logger.Trace("Got response. request-id={0}, status-code={1}", response.RequestId, response.StatusCode);

				return response;
			}
			finally
			{
				webResponse?.Dispose();
			}
		}

		private async Task<HttpWebRequest> CreateOnPremiseTargetWebRequestAsync(string url, IOnPremiseTargetRequest onPremiseTargetRequest)
		{
			_logger.Trace("Creating web request");

			var uri = String.IsNullOrWhiteSpace(url) ? _baseUri : new Uri(_baseUri, url);
			var webRequest = WebRequest.CreateHttp(uri);
			webRequest.Method = onPremiseTargetRequest.HttpMethod;
			webRequest.Timeout = _requestTimeout*1000;

			foreach (var httpHeader in onPremiseTargetRequest.HttpHeaders)
			{
				_logger.Trace("   adding header. header={0}, value={1}", httpHeader.Key, httpHeader.Value);

				Action<HttpWebRequest, string> restrictedHeader;
				if (_requestHeaderTransformations.TryGetValue(httpHeader.Key, out restrictedHeader))
				{
					restrictedHeader(webRequest, httpHeader.Value);
				}
				else
				{
					webRequest.Headers.Add(httpHeader.Key, httpHeader.Value);
				}
			}

			if (onPremiseTargetRequest.Body != null)
			{
				_logger.Trace("   adding request body. length={0}", onPremiseTargetRequest.Body.Length);

				var requestStream = await webRequest.GetRequestStreamAsync();
				await requestStream.WriteAsync(onPremiseTargetRequest.Body, 0, onPremiseTargetRequest.Body.Length);
				await requestStream.FlushAsync();
			}

			return webRequest;
		}
	}
}