using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Serilog;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Http
{
	internal class HttpResponseMessageBuilder : IHttpResponseMessageBuilder
	{
		private readonly ILogger _logger;
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly Dictionary<string, Action<HttpContent, string>> _contentHeaderTransformation;

		public HttpResponseMessageBuilder(ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_logger = logger;
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));

			_contentHeaderTransformation = new Dictionary<string, Action<HttpContent, string>>()
			{
				["Content-Disposition"] = (r, v) => r.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(v),
				["Content-Length"] = (r, v) => r.Headers.ContentLength = Int64.Parse(v),
				["Content-Location"] = (r, v) => r.Headers.ContentLocation = new Uri(v),
				["Content-MD5"] = null,
				["Content-Range"] = null,
				["Content-Type"] = (r, v) => r.Headers.ContentType = MediaTypeHeaderValue.Parse(v),
				["Expires"] = (r, v) => r.Headers.Expires = (v == "-1" ? (DateTimeOffset?)null : new DateTimeOffset(DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture))),
				["Last-Modified"] = (r, v) => r.Headers.LastModified = new DateTimeOffset(DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture)),
			};
		}

		public HttpResponseMessage BuildFromConnectorResponse(IOnPremiseConnectorResponse response, Link link, string requestId)
		{
			var message = new HttpResponseMessage();

			if (response == null)
			{
				_logger?.Verbose("Received no response. request-id={request-id}", requestId);

				message.StatusCode = HttpStatusCode.GatewayTimeout;
				message.Content = new ByteArrayContent(new byte[0]);
				message.Content.Headers.Add("X-TTRELAY-TIMEOUT", "On-Premise");
			}
			else
			{
				message.StatusCode = response.StatusCode;
				message.Content = GetResponseContentForOnPremiseTargetResponse(response, link);

				if (response.HttpHeaders.TryGetValue("WWW-Authenticate", out var wwwAuthenticate))
				{
					var parts = wwwAuthenticate.Split(' ');
					message.Headers.WwwAuthenticate.Add(parts.Length == 2
						? new AuthenticationHeaderValue(parts[0], parts[1])
						: new AuthenticationHeaderValue(wwwAuthenticate)
					);
				}
			}

			return message;
		}

		public HttpContent GetResponseContentForOnPremiseTargetResponse(IOnPremiseConnectorResponse response, Link link)
		{
			if (response == null)
				throw new ArgumentNullException(nameof(response));

			if (response.StatusCode == HttpStatusCode.InternalServerError && !link.ForwardOnPremiseTargetErrorResponse)
			{
				return null;
			}

			HttpContent content;

			if (response.ContentLength == 0)
			{
				_logger?.Verbose("Received no body. request-id={request-id}", response.RequestId);

				content = new ByteArrayContent(new byte[0]);
			}
			else if (response.Body != null)
			{
				_logger?.Verbose("Received small body with data. request-id={request-id}, body-length={response-content-length}", response.RequestId, response.Body.Length);

				content = new ByteArrayContent(response.Body);
			}
			else
			{
				_logger?.Verbose("Received body. request-id={request-id}, content-length={response-content-length}", response.RequestId, response.ContentLength);

				var stream = _postDataTemporaryStore.GetResponseStream(response.RequestId);
				if (stream == null)
				{
					throw new InvalidOperationException(); // TODO what now?
				}

				content = new StreamContent(stream);
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
				if (_contentHeaderTransformation.TryGetValue(httpHeader.Key, out var contentHeaderTranformation))
				{
					contentHeaderTranformation?.Invoke(content, httpHeader.Value);
				}
				else
				{
					content.Headers.TryAddWithoutValidation(httpHeader.Key, httpHeader.Value);
				}
			}
		}
	}
}
