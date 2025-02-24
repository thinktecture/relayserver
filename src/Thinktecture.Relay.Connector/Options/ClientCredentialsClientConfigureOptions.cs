using System;
using Duende.AccessTokenManagement;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Connector.Options;

internal partial class ClientCredentialsClientConfigureOptions : IConfigureNamedOptions<ClientCredentialsClient>
{
	private readonly ILogger<ClientCredentialsClientConfigureOptions> _logger;
	private readonly RelayConnectorOptions _relayConnectorOptions;

	public ClientCredentialsClientConfigureOptions(ILogger<ClientCredentialsClientConfigureOptions> logger,
		IOptions<RelayConnectorOptions> relayConnectorOptions)
	{
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger;
		_relayConnectorOptions = relayConnectorOptions.Value;
	}

	public void Configure(string? name, ClientCredentialsClient options)
	{
		if (name != Constants.RelayServerClientName)
		{
			return;
		}

		Log.UseTokenEndpoint(_logger, _relayConnectorOptions.DiscoveryDocument.AuthorizationTokenEndpoint);

		options.TokenEndpoint = _relayConnectorOptions.DiscoveryDocument.AuthorizationTokenEndpoint;
		options.ClientId = _relayConnectorOptions.TenantName;
		options.ClientSecret = _relayConnectorOptions.TenantSecret;
		options.Scope = Constants.RelayServerScopes;
	}

	public void Configure(ClientCredentialsClient options)
	{
		Configure(null, options);
	}
}
