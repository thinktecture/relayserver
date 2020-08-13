using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayServerConfigurationRetriever : IConfigurationRetriever<DiscoveryDocument>
	{
		private readonly ILogger<RelayServerConfigurationRetriever> _logger;

		public RelayServerConfigurationRetriever(ILogger<RelayServerConfigurationRetriever> logger)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		}

		public async Task<DiscoveryDocument> GetConfigurationAsync(string address, IDocumentRetriever retriever, CancellationToken cancel)
		{
			try
			{
				var document = await retriever.GetDocumentAsync(address, cancel);

				return JsonSerializer.Deserialize<DiscoveryDocument>(document, new JsonSerializerOptions()
				{
					PropertyNamingPolicy = JsonNamingPolicy.CamelCase
				});
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while fetching the discovery document.");
				throw;
			}
		}
	}
}
