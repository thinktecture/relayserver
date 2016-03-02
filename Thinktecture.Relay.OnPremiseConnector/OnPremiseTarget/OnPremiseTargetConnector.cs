using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using NLog.Interface;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    internal class OnPremiseTargetConnector : IOnPremiseTargetConnector
    {
        private static readonly Dictionary<string, Action<HttpWebRequest, string>> _requestHeaderTransformations;

        static OnPremiseTargetConnector()
        {
            _requestHeaderTransformations = new Dictionary<string, Action<HttpWebRequest, string>>()
            {
                { "Accept", (r, v) => r.Accept = v },
                { "Connection", (r, v) => r.Connection = v },
                { "Content-Type", (r, v) => r.ContentType = v },
                { "Content-Length", (r, v) => r.ContentLength = Int64.Parse(v) },
                { "Date", (r, v) => r.Date = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture) },
                { "Expect", null },
                { "Host", null },
                { "If-Modified-Since", (r, v) => r.IfModifiedSince = DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture) },
                { "Proxy-Connection", null },
                { "Range", null },
                { "Referer", (r, v) => r.Referer = v },
                { "Transfer-Encoding", (r, v) => r.TransferEncoding = v },
                { "User-Agent", (r, v) => r.UserAgent = v }
            };
        }

        private readonly int _requestTimeout;
        private readonly ILogger _logger;

        public Uri BaseUri { get; private set; }

        public OnPremiseTargetConnector(Uri baseUri, int requestTimeout, ILogger logger)
        {
            BaseUri = baseUri;

            _requestTimeout = requestTimeout;
            _logger = logger;
        }

        public async Task<IOnPremiseTargetReponse> GetResponseAsync(string url, IOnPremiseTargetRequest onPremiseTargetRequest)
        {
            _logger.Debug("Requesting response from On-Premise Target...");
            _logger.Trace("Requesting response from On-Premise Target. url={0}, request-id={1}, origin-id", url, onPremiseTargetRequest.RequestId, onPremiseTargetRequest.OriginId);

            var onPremiseTargetReponse = new OnPremiseTargetReponse()
            {
                RequestId = onPremiseTargetRequest.RequestId,
                OriginId = onPremiseTargetRequest.OriginId,
                RequestStarted = DateTime.UtcNow
            };

            var webRequest = await CreateOnPremiseTargetWebRequestAsync(url, onPremiseTargetRequest);

            HttpWebResponse webResponse = null;
            try
            {
                try
                {
                    webResponse = (HttpWebResponse) await webRequest.GetResponseAsync();
                }
                catch (WebException wex)
                {
                    _logger.Trace("Error requesting response. request-id={0}", wex, onPremiseTargetRequest.RequestId);

                    if (wex.Status == WebExceptionStatus.ProtocolError)
                    {
                        webResponse = (HttpWebResponse) wex.Response;
                    }
                }

                if (webResponse == null)
                {
                    _logger.Warn("Gateway timeout!");
                    _logger.Trace("Gateway timeout. request-id={0}", onPremiseTargetRequest.RequestId);

                    onPremiseTargetReponse.StatusCode = HttpStatusCode.GatewayTimeout;
                    onPremiseTargetReponse.HttpHeaders = new Dictionary<string, string>()
                    {
                        { "X-TTRELAY-TIMEOUT", "On-Premise Target" }
                    };
                }
                else
                {
                    onPremiseTargetReponse.StatusCode = webResponse.StatusCode;

                    using (var stream = new MemoryStream())
                    {
                        onPremiseTargetReponse.HttpHeaders = webResponse.Headers.AllKeys.ToDictionary(n => n, n => webResponse.Headers.Get(n), StringComparer.OrdinalIgnoreCase);

                        using (var responseStream = webResponse.GetResponseStream() ?? Stream.Null)
                        {
                            await responseStream.CopyToAsync(stream);
                        }

                        onPremiseTargetReponse.Body = stream.ToArray();
                    }
                }

                onPremiseTargetReponse.RequestFinished = DateTime.UtcNow;

                _logger.Trace("Got response. status-code={0}, request-id={1}", onPremiseTargetReponse.StatusCode, onPremiseTargetReponse.RequestId);

                return onPremiseTargetReponse;
            }
            finally
            {
                if (webResponse != null)
                {
                    webResponse.Dispose();
                }
            }
        }

        private async Task<HttpWebRequest> CreateOnPremiseTargetWebRequestAsync(string url, IOnPremiseTargetRequest onPremiseTargetRequest)
        {
            _logger.Trace("Creating web request");

            var webRequest = WebRequest.CreateHttp(new Uri(BaseUri, url));
            webRequest.Method = onPremiseTargetRequest.HttpMethod;
            webRequest.Timeout = _requestTimeout * 1000;

            foreach (var httpHeader in onPremiseTargetRequest.HttpHeaders)
            {
                _logger.Trace("   adding header: header={0}, value={1}", httpHeader.Key, httpHeader.Value);

                Action<HttpWebRequest, string> restrictedHeader;
                if (_requestHeaderTransformations.TryGetValue(httpHeader.Key, out restrictedHeader))
                {
                    if (restrictedHeader != null)
                    {
                        restrictedHeader(webRequest, httpHeader.Value);
                    }
                }
                else
                {
                    webRequest.Headers.Add(httpHeader.Key, httpHeader.Value);
                }
            }

            if (onPremiseTargetRequest.Body != null)
            {
                _logger.Trace("   adding request body, length={0}", onPremiseTargetRequest.Body.Length);

                var requestStream = await webRequest.GetRequestStreamAsync();
                await requestStream.WriteAsync(onPremiseTargetRequest.Body, 0, onPremiseTargetRequest.Body.Length);
                await requestStream.FlushAsync();
            }

            return webRequest;
        }
    }
}
