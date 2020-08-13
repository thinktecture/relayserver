using System.Threading;
using IdentityModel.AspNetCore.AccessTokenManagement;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Options
{
	internal class ConfigureAccessTokenManagementOptions<TRequest, TResponse> : IConfigureOptions<AccessTokenManagementOptions>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse

	{
		private readonly IOptions<RelayConnectorOptions<TRequest, TResponse>> _options;

		public ConfigureAccessTokenManagementOptions(IOptions<RelayConnectorOptions<TRequest, TResponse>> options)
		{
			_options = options;
		}

		public void Configure(AccessTokenManagementOptions options)
		{
			var relayConnectorOptions = _options.Value;

			var discoveryDocument = relayConnectorOptions.ConfigurationManager
				.GetConfigurationAsync(CancellationToken.None)
				.GetAwaiter().GetResult();

			options.Client.Clients.Add(Constants.RelayServerHttpClientName, new ClientCredentialsTokenRequest()
			{
				Address = discoveryDocument.AuthorizationServer + "/connect/token", // Todo: Get that from the oidc discovery document, too
				ClientId = relayConnectorOptions.TenantName,
				ClientSecret = relayConnectorOptions.TenantSecret,
				Scope = Constants.RelayServerScopes,
			});
		}
	}
}
