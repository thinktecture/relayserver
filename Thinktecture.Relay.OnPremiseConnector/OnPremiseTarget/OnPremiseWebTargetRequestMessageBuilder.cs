using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseWebTargetRequestMessageBuilder : IOnPremiseWebTargetRequestMessageBuilder
	{
		private static readonly string[] _ignoredHeaders =
		{
			"Host", "Connection", "Expect", "Proxy-Connection", "Proxy-Authorization",
			"Range", "If-Range", "TransferEncoding", "Transfer-Encoding-Chunked", "Upgrade", "Via", "Warning", "Trailer", "Pragma"
		};

		private static readonly Dictionary<string, Action<HttpRequestHeaders, string>> _requestHeadersTransformations;
		private static readonly Dictionary<string, Action<HttpContentHeaders, string>> _contentHeadersTransformations;

		private readonly ILogger _logger;

		static OnPremiseWebTargetRequestMessageBuilder()
		{
			_requestHeadersTransformations = new Dictionary<string, Action<HttpRequestHeaders, string>>
			{
				["Accept"] = (r, v) =>
				{
					if (MediaTypeWithQualityHeaderValue.TryParse(v, out var value)) r.Accept.Add(value);
				},
				["Accept-Charset"] = (r, v) =>
				{
					if (StringWithQualityHeaderValue.TryParse(v, out var value)) r.AcceptCharset.Add(value);
				},
				["Accept-Enconding"] = (r, v) =>
				{
					if (StringWithQualityHeaderValue.TryParse(v, out var value)) r.AcceptEncoding.Add(value);
				},
				["Accept-Language"] = (r, v) =>
				{
					if (StringWithQualityHeaderValue.TryParse(v, out var value)) r.AcceptLanguage.Add(value);
				},
				["Authorization"] = (r, v) =>
				{
					if (AuthenticationHeaderValue.TryParse(v, out var value)) r.Authorization = value;
				},
				["Cache-Control"] = (r, v) =>
				{
					if (CacheControlHeaderValue.TryParse(v, out var value)) r.CacheControl = value;
				},
				["Date"] = (r, v) =>
				{
					if (DateTimeOffset.TryParseExact(v, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) r.Date = value;
				},
				["If-Match"] = (r, v) =>
				{
					if (EntityTagHeaderValue.TryParse(v, out var value)) r.IfMatch.Add(value);
				},
				["If-Modified-Since"] = (r, v) =>
				{
					if (DateTimeOffset.TryParseExact(v, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) r.IfModifiedSince = value;
				},
				["If-None-Match"] = (r, v) =>
				{
					if (EntityTagHeaderValue.TryParse(v, out var value)) r.IfNoneMatch.Add(value);
				},
				["If-Unmodified-Since"] = (r, v) =>
				{
					if (DateTimeOffset.TryParseExact(v, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) r.IfUnmodifiedSince = value;
				},
				["Max-Forwards"] = (r, v) =>
				{
					if (Int32.TryParse(v, out var value)) r.MaxForwards = value;
				},
				["Referer"] = (r, v) =>
				{
					if (Uri.TryCreate(v, UriKind.RelativeOrAbsolute, out var value)) r.Referrer = value;
				},
				["TE"] = (r, v) =>
				{
					if (TransferCodingWithQualityHeaderValue.TryParse(v, out var value)) r.TE.Add(value);
				},
				["User-Agent"] = (r, v) =>
				{
					if (ProductInfoHeaderValue.TryParse(v, out var value)) r.UserAgent.Add(value);
				},
			};
			_contentHeadersTransformations = new Dictionary<string, Action<HttpContentHeaders, string>>
			{
				["Allow"] = (r, v) => r.Allow.Add(v),
				["Content-Disposition"] = (r, v) =>
				{
					if (ContentDispositionHeaderValue.TryParse(v, out var value)) r.ContentDisposition = value;
				},
				["Content-Encoding"] = (r, v) => r.ContentEncoding.Add(v),
				["Content-Language"] = (r, v) => r.ContentLanguage.Add(v),
				["Content-Length"] = (r, v) =>
				{
					if (Int32.TryParse(v, out var value)) r.ContentLength = value;
				},
				["Content-Location"] = (r, v) =>
				{
					if (Uri.TryCreate(v, UriKind.RelativeOrAbsolute, out var value)) r.ContentLocation = value;
				},
				["Content-MD5"] = (r, v) => r.ContentMD5 = Encoding.ASCII.GetBytes(v),
				["Content-Type"] = (r, v) =>
				{
					if (MediaTypeHeaderValue.TryParse(v, out var value)) r.ContentType = value;
				},
				["Expires"] = (r, v) =>
				{
					if (DateTimeOffset.TryParseExact(v, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) r.Expires = value;
				},
				["Last-Modified"] = (r, v) =>
				{
					if (DateTimeOffset.TryParseExact(v, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)) r.LastModified = value;
				},
			};
		}

		public OnPremiseWebTargetRequestMessageBuilder(ILogger logger)
		{
			_logger = logger;
		}

		public HttpRequestMessage CreateLocalTargetRequestMessage(Uri baseUri, string url, IOnPremiseTargetRequest request, string relayedRequestHeader, bool logSensitiveData)
		{
			_logger?.Verbose("Creating web request for request-id={RequestId}", request.RequestId);

			var message = new HttpRequestMessage(new HttpMethod(request.HttpMethod), String.IsNullOrWhiteSpace(url) ? baseUri : new Uri(baseUri, url));

			if (request.Stream != Stream.Null)
			{
				_logger?.Verbose("Adding request stream to request. request-id={RequestId}", request.RequestId);
				message.Content = new StreamContent(request.Stream, 0x10000);
			}

			if (request.AcknowledgmentMode == AcknowledgmentMode.Manual)
			{
				_logger?.Verbose("Request needs to be manually acknowledged, adding header. request-id={RequestId}, acknowledgment-mode={AcknowledgmentMode}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgmentMode, request.AcknowledgeId);
				message.Headers.Add("X-TTRELAY-ACKNOWLEDGE-ID", request.AcknowledgeId);
			}

			foreach (var httpHeader in request.HttpHeaders.Where(kvp => _ignoredHeaders.All(name => name != kvp.Key)))
			{
				_logger?.Verbose("Adding header to request. request-id={RequestId} header-name={HeaderName}, header-value={HeaderValue}", request.RequestId, httpHeader.Key, logSensitiveData ? httpHeader.Value : "***");

				try
				{
					if (_requestHeadersTransformations.TryGetValue(httpHeader.Key, out var requestHeader))
					{
						requestHeader(message.Headers, httpHeader.Value);
					}
					else if (_contentHeadersTransformations.TryGetValue(httpHeader.Key, out var contentHeader))
					{
						if (request.Stream != Stream.Null)
						{
							contentHeader(message.Content.Headers, httpHeader.Value);
						}
					}
					else
					{
						message.Headers.Add(httpHeader.Key, httpHeader.Value);
					}
				}
				catch (Exception ex)
				{
					_logger?.Error(ex, "Could not add header. header-name={HeaderName}", httpHeader.Key);
				}
			}

			if (!String.IsNullOrWhiteSpace(relayedRequestHeader))
			{
				message.Headers.Add(relayedRequestHeader, "true");
			}

			return message;
		}
	}
}
