using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Http
{
	internal class HttpResponseMessageBuilder : IHttpResponseMessageBuilder
	{
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly Dictionary<string, Action<HttpContent, string>> _contentHeaderTransformation;

		public HttpResponseMessageBuilder(IPostDataTemporaryStore postDataTemporaryStore)
		{
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

		public HttpResponseMessage BuildFrom(IOnPremiseTargetResponse onPremiseTargetResponse, Link link)
		{
			var response = new HttpResponseMessage();

			if (onPremiseTargetResponse == null)
			{
				response.StatusCode = HttpStatusCode.GatewayTimeout;
				response.Content = new ByteArrayContent(new byte[] { });
				response.Content.Headers.Add("X-TTRELAY-TIMEOUT", "On-Premise");
			}
			else
			{
				response.StatusCode = onPremiseTargetResponse.StatusCode;
				response.Content = GetResponseContentForOnPremiseTargetResponse(onPremiseTargetResponse, link);

				if (onPremiseTargetResponse.HttpHeaders.TryGetValue("WWW-Authenticate", out var wwwAuthenticate))
				{
					var parts = wwwAuthenticate.Split(' ');
					response.Headers.WwwAuthenticate.Add((parts.Length == 2)
						? new AuthenticationHeaderValue(parts[0], parts[1])
						: new AuthenticationHeaderValue(wwwAuthenticate)
					);
				}
			}

			return response;
		}

		internal HttpContent GetResponseContentForOnPremiseTargetResponse(IOnPremiseTargetResponse onPremiseTargetResponse, Link link)
		{
			if (onPremiseTargetResponse == null)
				throw new ArgumentNullException(nameof(onPremiseTargetResponse), "On-premise target response cannot be null");

			if (onPremiseTargetResponse.StatusCode == HttpStatusCode.InternalServerError && !link.ForwardOnPremiseTargetErrorResponse)
			{
				return null;
			}

			HttpContent content;

			var stream = _postDataTemporaryStore.GetResponseStream(onPremiseTargetResponse.RequestId);
			if (stream != null)
			{
				content = new StreamContent(stream);
			}
			else
			{
				content = new ByteArrayContent(onPremiseTargetResponse.Body ?? new byte[] { });
			}

			SetHttpHeaders(onPremiseTargetResponse.HttpHeaders, content);
			return content;
		}

		internal void SetHttpHeaders(IReadOnlyDictionary<string, string> httpHeaders, HttpContent content)
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
