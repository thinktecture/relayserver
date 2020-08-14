using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Protocols;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorPostConfigureOptions : IPostConfigureOptions<RelayConnectorOptions>
	{
		private readonly IServiceProvider _serviceProvider;

		public RelayConnectorPostConfigureOptions(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		public void PostConfigure(string name, RelayConnectorOptions options)
		{
			var uri = new Uri(options.RelayServerBaseUri, Constants.WellKnownDiscoveryDocumentPath);

			var configManager = new ConfigurationManager<DiscoveryDocument>(
				uri.ToString(),
				ActivatorUtilities.CreateInstance<RelayServerConfigurationRetriever>(_serviceProvider),
				new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https", }
			);

			options.DiscoveryDocument = configManager.GetConfigurationAsync()
				.GetAwaiter().GetResult();
		}
	}
}
