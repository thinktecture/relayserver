using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Protocols;

namespace Thinktecture.Relay.Connector.Options
{
	internal class RelayConnectorPostConfigureOptions : IPostConfigureOptions<RelayConnectorOptions>
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IApplicationLifetime _applicationLifetime;

		public RelayConnectorPostConfigureOptions(IServiceProvider serviceProvider, IApplicationLifetime applicationLifetime)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
		}

		public void PostConfigure(string name, RelayConnectorOptions options)
		{
			var uri = new Uri(options.RelayServerBaseUri, DiscoveryDocument.WellKnownPath);

			while (!_applicationLifetime.ApplicationStopping.IsCancellationRequested)
			{
				var configManager = new ConfigurationManager<DiscoveryDocument>(
					uri.ToString(),
					ActivatorUtilities.CreateInstance<RelayServerConfigurationRetriever>(_serviceProvider),
					new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https", }
				);

				try
				{
					options.DiscoveryDocument = configManager.GetConfigurationAsync().GetAwaiter().GetResult();
					break;
				}
				catch
				{
					Console.WriteLine($"Could not get discovery document from {uri}");
					Task.Delay(TimeSpan.FromSeconds(10)).GetAwaiter().GetResult();
				}
			}
		}
	}
}
