using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Http
{
	internal class HttpResponseMessageBuilder : IHttpResponseMessageBuilder
	{
		private readonly Dictionary<string, Action<HttpContent, string>> _contentHeaderTransformation;

		public HttpResponseMessageBuilder()
		{
			_contentHeaderTransformation = new Dictionary<string, Action<HttpContent, string>>
			{
				{ "Content-Disposition", (r, v) => r.Headers.ContentDisposition = ContentDispositionHeaderValue.Parse(v) },
				{ "Content-Length", (r, v) => r.Headers.ContentLength = long.Parse(v) },
				{ "Content-Location", (r, v) => r.Headers.ContentLocation = new Uri(v) },
				{ "Content-MD5", null },
				{ "Content-Range", null },
				{ "Content-Type", (r, v) => r.Headers.ContentType = MediaTypeHeaderValue.Parse(v) },
				{ "Expires", (r, v) => r.Headers.Expires = v == "-1" ? (DateTimeOffset?) null : new DateTimeOffset(DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture)) },
				{ "Last-Modified", (r, v) => r.Headers.LastModified = new DateTimeOffset(DateTime.ParseExact(v, "R", CultureInfo.InvariantCulture)) }
			};
		}

		public HttpResponseMessage BuildFrom(IOnPremiseTargetReponse onPremiseTargetReponse, Link link)
		{
			var response = new HttpResponseMessage();

			if (onPremiseTargetReponse == null)
			{
				response.StatusCode = HttpStatusCode.GatewayTimeout;
				response.Content = new ByteArrayContent(new byte[] { });
				response.Content.Headers.Add("X-TTRELAY-TIMEOUT", "On-Premise");
			}
			else
			{
				response.StatusCode = onPremiseTargetReponse.StatusCode;
				response.Content = GetResponseContentForOnPremiseTargetResponse(onPremiseTargetReponse, link);
			}

			return response;
		}

		internal HttpContent GetResponseContentForOnPremiseTargetResponse(IOnPremiseTargetReponse onPremiseTargetReponse, Link link)
		{
			if (onPremiseTargetReponse == null)
			{
                throw new ArgumentNullException(nameof(onPremiseTargetReponse), "On-Premise Target response must not be null here.");
			}

			if (onPremiseTargetReponse.StatusCode == HttpStatusCode.InternalServerError &&
				!link.ForwardOnPremiseTargetErrorResponse)
			{
				return null;
			}

			var content = new ByteArrayContent(onPremiseTargetReponse.Body ?? new byte[] { });
			SetHttpHeaders(onPremiseTargetReponse.HttpHeaders, content);

			return content;
		}

		internal void SetHttpHeaders(IDictionary<string, string> httpHeaders, HttpContent content)
		{
			if (httpHeaders == null)
			{
				return;
			}

			foreach (var httpHeader in httpHeaders)
			{
				Action<HttpContent, string> contentHeaderTranformation;

				if (_contentHeaderTransformation.TryGetValue(httpHeader.Key, out contentHeaderTranformation))
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