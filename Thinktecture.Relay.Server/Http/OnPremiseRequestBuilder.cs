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
		private static readonly string[] _ignoredHeaders = {"Host", "Connection"};

		private readonly IPostDataTemporaryStore _postDataTemporaryStore;
		private readonly ILogger _logger;

		public OnPremiseRequestBuilder(ILogger logger, IPostDataTemporaryStore postDataTemporaryStore)
		{
			_postDataTemporaryStore = postDataTemporaryStore ?? throw new ArgumentNullException(nameof(postDataTemporaryStore));
			_logger = logger;
		}

		public async Task<IOnPremiseConnectorRequest> BuildFromHttpRequest(HttpRequestMessage message, Guid originId, string pathWithoutUserName)
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
				}
			}


			request.HttpHeaders = message.Content.Headers
				.Where(kvp => _ignoredHeaders.All(name => name != kvp.Key))
				.Select(kvp => new { Name = kvp.Key, Value = CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(kvp.Value) })
				.ToDictionary(header => header.Name, header => header.Value);

			return request;
		}

		internal string CombineMultipleHttpHeaderValuesIntoOneCommaSeperatedValue(IEnumerable<string> headers)
		{
			// HTTP RFC2616 says, that multiple headers can be combined into a comma-separated single header
			return headers.Aggregate(String.Empty, (s, v) => s + (String.IsNullOrWhiteSpace(s) ? String.Empty : ", ") + v);
		}
	}
}
