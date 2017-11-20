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
		private static readonly string[] _ignoredHeaders = { "Host", "Connection" };

		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly ILogger _logger;

		public OnPremiseRequestBuilder(ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
			_logger = logger;
		}

		public async Task<IOnPremiseConnectorRequest> BuildFromHttpRequest(HttpRequestMessage message, Guid originId, string pathWithoutUserName, string basePath)
		{
			var request = new OnPremiseConnectorRequest
			{
				RequestId = Guid.NewGuid().ToString(),
				HttpMethod = message.Method.Method,
				Url = pathWithoutUserName + message.RequestUri.Query,
				HttpHeaders = message.Headers.ToDictionary(kvp => kvp.Key, kvp => CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value), StringComparer.OrdinalIgnoreCase),
				OriginId = originId,
				RequestStarted = DateTime.UtcNow
			};

			if (message.Content.Headers.ContentLength.GetValueOrDefault(0x10000) >= 0x10000)
			{
				var contentStream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);

				using (var storeStream = _postDataTemporaryStore.CreateRequestStream(request.RequestId))
				{
					await contentStream.CopyToAsync(storeStream).ConfigureAwait(false);
					if (storeStream.Length < 0x10000)
					{
						if (storeStream.Length == 0)
						{
							// no body available (e.g. GET request)
						}
						else
						{
							// the body is small enough to be used directly
							request.Body = new byte[storeStream.Length];
							storeStream.Position = 0;
							await storeStream.ReadAsync(request.Body, 0, (int)storeStream.Length).ConfigureAwait(false);
						}

						// TODO delete obsolete file now
					}
					else
					{
						// a length of 0 indicates that there is a larger body available on the server
						request.Body = new byte[0];
					}

					request.ContentLength = storeStream.Length;
				}
			}
			else if (message.Content.Headers.ContentLength.GetValueOrDefault(0) > 0)
			{
				// we have a body, and it is small enough to be transmitted directly
				var contentStream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false);

				// ReSharper disable once PossibleInvalidOperationException
				request.Body = new byte[message.Content.Headers.ContentLength.Value];
				await contentStream.ReadAsync(request.Body, 0, (int)message.Content.Headers.ContentLength.Value).ConfigureAwait(false);
			}

			var headers = message.Headers
				.Union(message.Content.Headers)
				.Where(kvp => _ignoredHeaders.All(name => name != kvp.Key))
				.Select(kvp => new { Name = kvp.Key, Value = CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value) })
				.ToDictionary(header => header.Name, header => header.Value);
			ApplyForwardedHeader(headers, request, message, basePath);
			request.HttpHeaders = headers;

			return request;
		}

		internal string CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(IEnumerable<string> headers)
		{
			// HTTP RFC2616 says, that multiple headers can be combined into a comma-separated single header
			return headers.Aggregate(String.Empty, (s, v) => s + (String.IsNullOrWhiteSpace(s) ? String.Empty : ", ") + v);
		}

		internal void ApplyForwardedHeader(IDictionary<string, string> headers, OnPremiseConnectorRequest request, HttpRequestMessage message, string forwardedPath)
		{
			const string forwardedHeader = "Forwarded";
			var forwardedProto = message.RequestUri.Scheme;
			var forwardedHost = message.RequestUri.Host;
			var forwardedPort = message.RequestUri.Port.ToString();

			// Only include port if it is non-standard to reduce length of header
			if (forwardedProto == "http" && forwardedPort == "80" || forwardedProto == "https" && forwardedPort == "443")
				forwardedPort = String.Empty;
			else
				forwardedPort = $":{forwardedPort}";

			// Add with Forwarded HTTP Extension RFC7239
			var forwardedHeaderValue = $"host={forwardedHost}{forwardedPort},proto={forwardedProto},path={forwardedPath}";
			if (!headers.ContainsKey(forwardedHeader))
			{
				headers.Add(forwardedHeader, forwardedHeaderValue);
			}
			else
			{
				var existingForwardedHeader = headers["Forwarded"]
					.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries)
					.Select(p => p.Split('='))
					.ToDictionary(kvp => kvp[0].Trim(), kvp => kvp[1].Trim());
				if (existingForwardedHeader.ContainsKey("host"))
					return;
				// host parameter is missing so add it and replace all other parameters too as they are useless without host
				existingForwardedHeader["host"] = $"{forwardedHost}{forwardedPort}";
				existingForwardedHeader["proto"] = forwardedProto;
				existingForwardedHeader["path"] = forwardedPath;
				headers["Forwarded"] = String.Join(",", existingForwardedHeader.Select(kvp => String.Format($"{kvp.Key}={kvp.Value}")));
			}
		}
	}
}
