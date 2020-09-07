using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Thinktecture.Relay.Connector.Options
{
	internal class ConfigureAccessTokenManagementOptions : IConfigureOptions<AccessTokenManagementOptions>
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly RelayConnectorOptions _options;

		public ConfigureAccessTokenManagementOptions(IServiceProvider serviceProvider, IOptions<RelayConnectorOptions> options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_options = options.Value;
		}

		public void Configure(AccessTokenManagementOptions options)
		{
			var uri = new Uri(new Uri(_options.DiscoveryDocument.AuthorizationServer), OidcConstants.Discovery.DiscoveryEndpoint);

			while (true)
			{
				var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
					uri.ToString(),
					ActivatorUtilities.CreateInstance<OpenIdConnectConfigurationRetriever>(_serviceProvider),
					new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https", }
				);

				try
				{
					var configuration = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();

					options.Client.Clients.Add(Constants.RelayServerHttpClientName, new ClientCredentialsTokenRequest()
					{
						Address = configuration.TokenEndpoint,
						ClientId = _options.TenantName,
						ClientSecret = _options.TenantSecret,
						Scope = Constants.RelayServerScopes,
					});
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
