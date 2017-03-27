﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	internal class OnPremiseRequestBuilder : IOnPremiseRequestBuilder
	{
		private readonly string[] _ignoredHeaders;

		public OnPremiseRequestBuilder()
		{
			_ignoredHeaders = new[] { "Host", "Connection" };
		}

		public async Task<IOnPremiseConnectorRequest> BuildFrom(HttpRequestMessage request, string originId, string pathWithoutUserName)
		{
			var onPremiseConnectorRequest = new OnPremiseConnectorRequest
			{
				RequestId = Guid.NewGuid().ToString(),

				Body = await GetClientRequestBodyAsync(request.Content),

				HttpMethod = request.Method.Method,
				Url = pathWithoutUserName + request.RequestUri.Query,

				HttpHeaders = request.Headers.ToDictionary(kvp => kvp.Key, kvp => CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value), StringComparer.OrdinalIgnoreCase),

				OriginId = originId,

				RequestStarted = DateTime.UtcNow
			};

			AddContentHeaders(onPremiseConnectorRequest, request);
			RemoveIgnoredHeaders(onPremiseConnectorRequest);

			return onPremiseConnectorRequest;
		}

		internal async Task<byte[]> GetClientRequestBodyAsync(HttpContent content)
		{
			var body = await content.ReadAsByteArrayAsync();

			if (body.LongLength == 0L)
			{
				return null;
			}

			return body;
		}

		internal string CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(IEnumerable<string> headers)
		{
			// HTTP RFC2616 says, that multiple headers can be combined into a comma-separated single header

			return headers.Aggregate(string.Empty, (s, v) => s + (s == string.Empty ? string.Empty : ", ") + v);
		}

		internal void AddContentHeaders(IOnPremiseConnectorRequest onPremiseConnectorRequest, HttpRequestMessage request)
		{
			foreach (var httpHeader in request.Content.Headers)
			{
				onPremiseConnectorRequest.HttpHeaders.Add(httpHeader.Key, CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(httpHeader.Value));
			}
		}

		internal void RemoveIgnoredHeaders(OnPremiseConnectorRequest onPremiseConnectorRequest)
		{
			foreach (var key in _ignoredHeaders)
			{
				onPremiseConnectorRequest.HttpHeaders.Remove(key);
			}
		}
	}
}