using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	internal class SignalRConnectionFactory
	{
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly RelayConnectorOptions _options;

		public SignalRConnectionFactory(IAccessTokenProvider accessTokenProvider, IOptions<RelayConnectorOptions> options)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));

			_options = options.Value;
		}

		public HubConnection CreateConnection()
		{
			return new HubConnectionBuilder()
				.WithUrl(new Uri(_options.DiscoveryDocument.ConnectorEndpoint),
					connectionOptions => { connectionOptions.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync; })
				.WithAutomaticReconnect() // TODO: Add retry policy based on discovery document config values
				.Build();
		}
	}
}
