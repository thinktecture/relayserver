using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	public class ConnectionFactory
	{
		private readonly ILogger<ConnectionFactory> _logger;
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly RelayConnectorOptions _options;

		public ConnectionFactory(ILogger<ConnectionFactory> logger, IAccessTokenProvider accessTokenProvider,
			IOptions<RelayConnectorOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
			_options = options?.Value ?? throw new ArgumentNullException(nameof(options));
		}

		public HubConnection CreateConnection()
		{
			_logger.LogDebug("Creating connection to {ConnectorEndpoint}", _options.DiscoveryDocument.ConnectorEndpoint);

			return new HubConnectionBuilder()
				.WithUrl(new Uri(_options.DiscoveryDocument.ConnectorEndpoint),
					options => options.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync)
				.WithAutomaticReconnect() // TODO add retry policy based on discovery document config values
				.Build();
		}
	}
}
