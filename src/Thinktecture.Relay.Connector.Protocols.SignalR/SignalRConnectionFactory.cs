using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	internal class SignalRConnectionFactory
	{
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly IOptions<RelayConnectorOptions> _options;

		private RelayConnectorOptions ConnectorOptions => _options.Value;

		public SignalRConnectionFactory(IAccessTokenProvider accessTokenProvider,
			IOptions<RelayConnectorOptions> options)
		{
			_accessTokenProvider = accessTokenProvider;
			_options = options;
		}

		public HubConnection CreateConnection()
		{
			var configuration = ConnectorOptions.DiscoveryDocument;

			return new HubConnectionBuilder()
				.WithUrl(new Uri(configuration.ConnectorEndpoint), connectionOptions =>
				{
					connectionOptions.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync;
				})
				.WithAutomaticReconnect() // TODO: Add retry policy based on discovery document config values
				.Build();
		}
	}
}
