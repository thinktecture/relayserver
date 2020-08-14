using System;
using System.Threading;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options
{
	internal class ConfigureAccessTokenManagementOptions<TRequest, TResponse> : IConfigureOptions<AccessTokenManagementOptions>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IOptions<RelayConnectorOptions<TRequest, TResponse>> _options;
		private readonly IServiceProvider _serviceProvider;

		public ConfigureAccessTokenManagementOptions(IOptions<RelayConnectorOptions<TRequest, TResponse>> options,
			IServiceProvider serviceProvider)
		{
			_options = options;
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		public void Configure(AccessTokenManagementOptions options)
		{
			var relayConnectorOptions = _options.Value;

			var uri = new Uri(new Uri(relayConnectorOptions.DiscoveryDocument.AuthorizationServer), ".well-known/openid-configuration");

			var configManager = new ConfigurationManager<OpenIdConnectConfiguration>(
				uri.ToString(),
				ActivatorUtilities.CreateInstance<OpenIdConnectConfigurationRetriever>(_serviceProvider),
				new HttpDocumentRetriever() { RequireHttps = uri.Scheme == "https", }
			);

			var configuration = configManager
				.GetConfigurationAsync(CancellationToken.None)
				.GetAwaiter().GetResult();

			options.Client.Clients.Add(Constants.RelayServerHttpClientName, new ClientCredentialsTokenRequest()
			{
				Address = configuration.TokenEndpoint,
				ClientId = relayConnectorOptions.TenantName,
				ClientSecret = relayConnectorOptions.TenantSecret,
				Scope = Constants.RelayServerScopes,
			});
		}
	}
}
