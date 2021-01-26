using System;
using System.Threading;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Thinktecture.Relay.Connector.Options
{
	internal class AccessTokenManagementConfigureOptions : IConfigureOptions<AccessTokenManagementOptions>
	{
		private readonly RelayConnectorOptions _relayConnectorOptions;
		private readonly ILogger<AccessTokenManagementConfigureOptions> _logger;
		private readonly IApplicationLifetime _applicationLifetime;

		public AccessTokenManagementConfigureOptions(ILogger<AccessTokenManagementConfigureOptions> logger,
			IApplicationLifetime applicationLifetime, IOptions<RelayConnectorOptions> relayConnectorOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_applicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
			_relayConnectorOptions = relayConnectorOptions?.Value ?? throw new ArgumentNullException(nameof(relayConnectorOptions));
		}

		public void Configure(AccessTokenManagementOptions options)
		{
			var uri = new Uri(new Uri(_relayConnectorOptions.DiscoveryDocument.AuthorizationServer),
				OidcConstants.Discovery.DiscoveryEndpoint);

			while (!_applicationLifetime.ApplicationStopping.IsCancellationRequested)
			{
				var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(uri.ToString(),
					new OpenIdConnectConfigurationRetriever(), new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https" });

				try
				{
					var configuration = configManager.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult();
					_logger.LogTrace("Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})", uri, configuration);

					options.Client.Clients.Add(Constants.HttpClientNames.RelayServer, new ClientCredentialsTokenRequest()
					{
						Address = configuration.TokenEndpoint,
						ClientId = _relayConnectorOptions.TenantName,
						ClientSecret = _relayConnectorOptions.TenantSecret,
						Scope = Constants.RelayServerScopes,
					});
					break;
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "An error occured while retrieving the discovery document from {DiscoverDocumentUrl}", uri);

					try
					{
						Task.Delay(TimeSpan.FromSeconds(10), _applicationLifetime.ApplicationStopping).GetAwaiter().GetResult();
					}
					catch (OperationCanceledException)
					{
						// Ignore this, as this will be thrown when the service shuts down gracefully
					}
				}
			}
		}
	}
}
