using System;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Connector.Authentication;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

/// <summary>
/// An implementation of a factory to create an instance of the <see cref="HubConnection"/> class.
/// </summary>
public partial class HubConnectionFactory
{
	private readonly IAccessTokenProvider _accessTokenProvider;
	private readonly ILogger _logger;
	private readonly RelayConnectorOptions _relayConnectorOptions;
	private readonly DiscoveryDocumentRetryPolicy _retryPolicy;

	/// <summary>
	/// Initializes a new instance of the <see cref="HubConnectionFactory"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="accessTokenProvider">An <see cref="IAccessTokenProvider"/>.</param>
	/// <param name="retryPolicy">The <see cref="DiscoveryDocumentRetryPolicy"/>.</param>
	/// <param name="relayConnectorOptions">An <see cref="IOptions{TOptions}"/>.</param>
	public HubConnectionFactory(ILogger<HubConnectionFactory> logger, IAccessTokenProvider accessTokenProvider,
		DiscoveryDocumentRetryPolicy retryPolicy, IOptions<RelayConnectorOptions> relayConnectorOptions)
	{
		if (relayConnectorOptions is null) throw new ArgumentNullException(nameof(relayConnectorOptions));

		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_accessTokenProvider = accessTokenProvider ?? throw new ArgumentNullException(nameof(accessTokenProvider));
		_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		_relayConnectorOptions = relayConnectorOptions.Value;
	}

	/// <summary>
	/// Creates a <see cref="HubConnection"/>.
	/// </summary>
	/// <returns>The <see cref="HubConnection"/>.</returns>
	public HubConnection Create()
	{
		Log.CreatingConnection(_logger, _relayConnectorOptions.DiscoveryDocument.ConnectorEndpoint);

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
