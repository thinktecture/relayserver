using System;
using System.Collections.Generic;
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
			// TODO: Directly stream to disk
			var onPremiseConnectorRequest = new OnPremiseConnectorRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				Body = await GetClientRequestBodyAsync(request.Content).ConfigureAwait(false),
				HttpMethod = request.Method.Method,
				Url = pathWithoutUserName + request.RequestUri.Query,
				HttpHeaders = request.Headers.ToDictionary(kvp => kvp.Key, kvp => CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value), StringComparer.OrdinalIgnoreCase),
				OriginId = originId,
				RequestStarted = DateTime.UtcNow,
			};

			// Store request body to file if it is larger than 64 kByte
			if ((onPremiseConnectorRequest.Body != null) && (onPremiseConnectorRequest.Body.Length >= 0x10000))
			{
				_postDataTemporaryStore.SaveRequest(onPremiseConnectorRequest.RequestId, onPremiseConnectorRequest.Body);
				onPremiseConnectorRequest.Body = new byte[] { };
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

		internal async Task<byte[]> GetClientRequestBodyAsync(HttpContent content)
		{
			var body = await content.ReadAsByteArrayAsync().ConfigureAwait(false);

			return (body.LongLength == 0L)
				? null
				: body;
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
