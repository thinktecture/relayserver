using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Protocols;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayServerConfigurationRetriever : IConfigurationRetriever<DiscoveryDocument>
	{
		public async Task<DiscoveryDocument> GetConfigurationAsync(string address, IDocumentRetriever retriever,
			CancellationToken cancellationToken = default)
		{
			var document = await retriever.GetDocumentAsync(address, cancellationToken);

			return JsonSerializer.Deserialize<DiscoveryDocument>(document, new JsonSerializerOptions()
			{
				PropertyNamingPolicy = JsonNamingPolicy.CamelCase
			});
		}
	}
}
