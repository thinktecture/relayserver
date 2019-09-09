using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	internal class OnPremiseRequestBuilder : IOnPremiseRequestBuilder
	{
		private static readonly string[] _ignoredHeaders = { "Host", "Connection" };

		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;

		public OnPremiseRequestBuilder(ILogger logger, IConfiguration configuration)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		}

		public async Task<IOnPremiseConnectorRequest> BuildFromHttpRequest(HttpRequestMessage message, Guid originId, string pathWithoutUserName)
		{
			var request = new OnPremiseConnectorRequest()
			{
				RequestId = Guid.NewGuid().ToString(),
				HttpMethod = message.Method.Method,
				Url = pathWithoutUserName + message.RequestUri.Query,
				HttpHeaders = message.Headers.ToDictionary(kvp => kvp.Key, kvp => CombineMultipleHttpHeaderValuesIntoOneCommaSeparatedValue(kvp.Value), StringComparer.OrdinalIgnoreCase),
				OriginId = originId,
				RequestStarted = DateTime.UtcNow,
				Expiration = _configuration.RequestExpiration,
				ContentLength = message.Content.Headers.ContentLength.GetValueOrDefault(0),
				Stream = await message.Content.ReadAsStreamAsync().ConfigureAwait(false),
			};

			request.HttpHeaders = message.Headers
				.Union(message.Content.Headers)
				.Where(kvp => _ignoredHeaders.All(name => name != kvp.Key))
				.Select(kvp => new { Name = kvp.Key, Value = CombineMultipleHttpHeaderValuesIntoOneCommaSeparatedValue(kvp.Value) })
				.ToDictionary(header => header.Name, header => header.Value);

			return request;
		}

		internal string CombineMultipleHttpHeaderValuesIntoOneCommaSeparatedValue(IEnumerable<string> headers)
		{
			// HTTP RFC2616 says, that multiple headers can be combined into a comma-separated single header
			return headers.Aggregate(String.Empty, (s, v) => s + (String.IsNullOrWhiteSpace(s) ? String.Empty : ", ") + v);
		}
	}
}
