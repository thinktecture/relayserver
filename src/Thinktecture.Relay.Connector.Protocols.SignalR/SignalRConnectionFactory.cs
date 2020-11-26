using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;
using Thinktecture.Relay.Connector.Options;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	/// <summary>
	/// An implementation of a factory to create an instance of the <see cref="HubConnection"/> class.
	/// </summary>
	public class ConnectionFactory
	{
		private readonly ILogger<ConnectionFactory> _logger;
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly RelayConnectorOptions _relayConnectorOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectionFactory"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="accessTokenProvider">An <see cref="IAccessTokenProvider"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public ConnectionFactory(ILogger<ConnectionFactory> logger, IAccessTokenProvider accessTokenProvider,
			IOptions<RelayConnectorOptions> relayConnectorOptions)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
			_relayConnectorOptions = relayConnectorOptions?.Value ?? throw new ArgumentNullException(nameof(relayConnectorOptions));
		}

		/// <summary>
		/// Creates a <see cref="HubConnection"/>.
		/// </summary>
		/// <returns>The <see cref="HubConnection"/>.</returns>
		public HubConnection CreateConnection()
		{
			_logger.LogDebug("Creating connection to {ConnectorEndpoint}", _relayConnectorOptions.DiscoveryDocument.ConnectorEndpoint);

			return new HubConnectionBuilder()
				.WithUrl(new Uri(_relayConnectorOptions.DiscoveryDocument.ConnectorEndpoint),
					options => options.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync)
				.WithAutomaticReconnect() // TODO add retry policy based on discovery document config values
				.Build();
		}
	}
}
