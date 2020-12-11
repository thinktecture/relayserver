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
	public class HubConnectionFactory
	{
		private readonly ILogger<HubConnectionFactory> _logger;
		private readonly IAccessTokenProvider _accessTokenProvider;
		private readonly DiscoveryDocumentRetryPolicy _retryPolicy;
		private readonly RelayConnectorOptions _relayConnectorOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="HubConnectionFactory"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="accessTokenProvider">An <see cref="IAccessTokenProvider"/>.</param>
		/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
		/// <param name="retryPolicy">The <see cref="DiscoveryDocumentRetryPolicy"/>.</param>
		public HubConnectionFactory(ILogger<HubConnectionFactory> logger, IAccessTokenProvider accessTokenProvider,
			IOptions<RelayConnectorOptions> relayConnectorOptions, DiscoveryDocumentRetryPolicy retryPolicy)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
			_relayConnectorOptions = relayConnectorOptions?.Value ?? throw new ArgumentNullException(nameof(relayConnectorOptions));
			_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		}

		/// <summary>
		/// Creates a <see cref="HubConnection"/>.
		/// </summary>
		/// <returns>The <see cref="HubConnection"/>.</returns>
		public HubConnection CreateConnection()
		{
			_logger.LogDebug("Creating connection to {ConnectorEndpoint}", _relayConnectorOptions.DiscoveryDocument.ConnectorEndpoint);

			var connection = new HubConnectionBuilder()
				.WithUrl(new Uri(_relayConnectorOptions.DiscoveryDocument.ConnectorEndpoint),
					options => options.AccessTokenProvider = _accessTokenProvider.GetAccessTokenAsync)
				.WithAutomaticReconnect(_retryPolicy)
				.Build();

			connection.HandshakeTimeout = _relayConnectorOptions.DiscoveryDocument.HandshakeTimeout;
			connection.SetKeepAliveInterval(_relayConnectorOptions.DiscoveryDocument.KeepAliveInterval);

			return connection;
		}
	}
}
