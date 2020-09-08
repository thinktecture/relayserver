using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	internal class ConnectionFactory
	{
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly ILogger<ConnectionFactory> _logger;
		private readonly RelayConnectorOptions _options;

		public ConnectionFactory(IAccessTokenProvider accessTokenProvider, IOptions<RelayConnectorOptions> options, ILogger<ConnectionFactory> logger)
		{
			if (options == null)
			{
				throw new ArgumentNullException(nameof(options));
			}

			_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_options = options.Value;
		}

		public HubConnection CreateConnection()
		{
			_logger.LogDebug("Creating connection to {ConnectorEndpoint}", _options.DiscoveryDocument.ConnectorEndpoint);
			return new HubConnectionBuilder()
				.WithUrl(new Uri(_options.DiscoveryDocument.ConnectorEndpoint),
					connectionOptions => { connectionOptions.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync; })
				.WithAutomaticReconnect() // TODO add retry policy based on discovery document config values
				.Build();
		}
	}
}
