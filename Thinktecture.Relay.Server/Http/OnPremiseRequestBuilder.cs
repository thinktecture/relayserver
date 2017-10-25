using System;
using System.Collections.Generic;
using System.IdentityModel.Protocols.WSTrust;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.Http
{
	internal class OnPremiseRequestBuilder : IOnPremiseRequestBuilder
	{
		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly ILogger _logger;
		private readonly string[] _ignoredHeaders;

		public OnPremiseRequestBuilder(ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
			_logger = logger;
			_ignoredHeaders = new[] { "Host", "Connection" };
		}

		public async Task<IOnPremiseConnectorRequest> BuildFrom(HttpRequestMessage request, Guid originId, string pathWithoutUserName)
		{
			var onPremiseConnectorRequest = new OnPremiseConnectorRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				HttpMethod = request.Method.Method,
				Url = pathWithoutUserName + request.RequestUri.Query,
				HttpHeaders = request.Headers.ToDictionary(kvp => kvp.Key, kvp => CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value), StringComparer.OrdinalIgnoreCase),
				OriginId = originId,
				RequestStarted = DateTime.UtcNow,
			};

			//if (request.Content != null)
			{
				if (request.Content.Headers.ContentLength.GetValueOrDefault(0x10000) >= 0x10000)
				{
					var content = await request.Content.ReadAsStreamAsync().ConfigureAwait(false);

					using (var stream = _postDataTemporaryStore.CreateRequestStream(onPremiseConnectorRequest.RequestId))
					{
						await content.CopyToAsync(stream).ConfigureAwait(false);
						if (stream.Length < 0x10000)
						{
							onPremiseConnectorRequest.Body = new byte[stream.Length];
							stream.Position = 0;
							await stream.ReadAsync(onPremiseConnectorRequest.Body, 0, (int)stream.Length).ConfigureAwait(false);
							// TODO delete obsolete file now
						}
						else
						{
							onPremiseConnectorRequest.Body = new byte[0];
						}
					}
				}
			}

			try
			{
				onPremiseConnectorRequest.ClientIpAddress = request.GetRemoteIpAddress();
			}
			catch (Exception ex)
			{
				_logger?.Warn(ex, "Could not fetch remote IP address for request {0}", onPremiseConnectorRequest.RequestId);
			}

			AddContentHeaders(onPremiseConnectorRequest, request);
			RemoveIgnoredHeaders(onPremiseConnectorRequest);

			return onPremiseConnectorRequest;
		}

		internal string CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(IEnumerable<string> headers)
		{
			// HTTP RFC2616 says, that multiple headers can be combined into a comma-separated single header
			return headers.Aggregate(String.Empty, (s, v) => s + (String.IsNullOrWhiteSpace(s) ? String.Empty : ", ") + v);
		}

		internal void AddContentHeaders(IOnPremiseConnectorRequest onPremiseConnectorRequest, HttpRequestMessage request)
		{
			foreach (var httpHeader in request.Content.Headers)
			{
				onPremiseConnectorRequest.HttpHeaders.Add(httpHeader.Key, CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(httpHeader.Value));
			}
		}

		internal void RemoveIgnoredHeaders(IOnPremiseConnectorRequest onPremiseConnectorRequest)
		{
			foreach (var key in _ignoredHeaders)
			{
				onPremiseConnectorRequest.HttpHeaders.Remove(key);
			}
		}
	}
}
