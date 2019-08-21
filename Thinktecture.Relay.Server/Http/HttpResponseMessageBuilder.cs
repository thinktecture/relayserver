using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Serilog;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	internal class HttpResponseMessageBuilder : IHttpResponseMessageBuilder
	{
		private readonly ILogger _logger;
		private readonly Dictionary<string, Action<HttpContent, string>> _contentHeaderTransformation;

		public HttpResponseMessageBuilder(ILogger logger)
		{
			_logger = logger;

			_contentHeaderTransformation = new Dictionary<string, Action<HttpContent, string>>()
			{
				["Content-Disposition"] = (r, v) => r.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(v),
				["Content-Length"] = (r, v) => r.Headers.ContentLength = Int64.Parse(v),
				["Content-Location"] = (r, v) => r.Headers.ContentLocation = new Uri(v),
				["Content-MD5"] = null,
				["Content-Range"] = null,
				["Content-Type"] = (r, v) => r.Headers.ContentType = MediaTypeHeaderValue.Parse(v),
				["Expires"] = (r, v) => r.Headers.Expires = SafeParseDateTime(r, v),
				["Last-Modified"] = (r, v) => r.Headers.LastModified = SafeParseDateTime(r, v),
			};
		}

		public HttpResponseMessage BuildFromConnectorResponse(IOnPremiseConnectorResponse response, bool forwardOnPremiseTargetErrorResponse, string requestId)
		{
			var message = new HttpResponseMessage();

			if (response == null)
			{
				_logger?.Verbose("Received no response. request-id={RequestId}", requestId);

				message.StatusCode = HttpStatusCode.GatewayTimeout;
				message.Content = new ByteArrayContent(Array.Empty<byte>());
				message.Content.Headers.Add("X-TTRELAY-TIMEOUT", "On-Premise");
			}
			else
			{
				message.StatusCode = response.StatusCode;
				message.Content = GetResponseContentForOnPremiseTargetResponse(response, forwardOnPremiseTargetErrorResponse);

				if (response.HttpHeaders.TryGetValue("WWW-Authenticate", out var wwwAuthenticate))
				{
					message.Headers.Add("WWW-Authenticate", wwwAuthenticate);
				}

				if (IsRedirectStatusCode(response.StatusCode) && response.HttpHeaders.TryGetValue("Location", out var location))
				{
					message.Headers.Location = new Uri(location, UriKind.RelativeOrAbsolute);
				}
			}

			return message;
		}

		public HttpContent GetResponseContentForOnPremiseTargetResponse(IOnPremiseConnectorResponse response, bool forwardOnPremiseTargetErrorResponse)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			if (response.StatusCode >= HttpStatusCode.InternalServerError && !forwardOnPremiseTargetErrorResponse)
			{
				return null;
			}

			HttpContent content;

			if (response.ContentLength == 0)
			{
				// No content
				content = new ByteArrayContent(Array.Empty<byte>());
			}
			else if (response.Body != null)
			{
				// Unmodified legacy response
				content = new ByteArrayContent(response.Body);
			}
			else
			{
				// Normal content stream
				var stream = response.Stream;
				if (stream == null)
				{
					throw new InvalidOperationException(); // TODO what now?
				}
				else if (stream.Position != 0 && stream.CanSeek)
				{
					stream.Position = 0;
				}

				content = new StreamContent(stream, 0x10000);
			}

			AddContentHttpHeaders(content, response.HttpHeaders);

			return content;
		}

		public void AddContentHttpHeaders(HttpContent content, IReadOnlyDictionary<string, string> httpHeaders)
		{
			if (httpHeaders == null)
			{
				return;
			}

			foreach (var httpHeader in httpHeaders)
			{
				if (_contentHeaderTransformation.TryGetValue(httpHeader.Key, out var contentHeaderTransformation))
				{
					contentHeaderTransformation?.Invoke(content, httpHeader.Value);
				}
				else
				{
					content.Headers.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
				}
			}
		}

		private static bool IsRedirectStatusCode(HttpStatusCode statusCode)
		{
			return ((int)statusCode >= 300) && ((int)statusCode <= 399);
		}

		private DateTimeOffset? SafeParseDateTime(HttpContent content, string value)
		{
			return DateTime.TryParseExact(value, "R", CultureInfo.InvariantCulture, DateTimeStyles.None, out var expiresValue)
				? new DateTimeOffset(expiresValue) : (DateTimeOffset?)null;
		}
	}
}
