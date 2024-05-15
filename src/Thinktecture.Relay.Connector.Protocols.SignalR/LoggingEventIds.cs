namespace Thinktecture.Relay.Connector.Protocols.SignalR;

internal static class LoggingEventIds
{
	public const int AcknowledgeTransportTransportingAck = 1;
	public const int AcknowledgeTransportErrorTransportingAck = 2;

	public const int ConnectorConnectionDisconnecting = 3;
	public const int ConnectorConnectionDisconnected = 4;
	public const int ConnectorConnectionGracefullyClosed = 5;
	public const int ConnectorConnectionReconnectingAfterLoss = 6;
	public const int ConnectorConnectionReconnectingAfterError = 7;
	public const int ConnectorConnectionReconnectedWithoutId = 9;
	public const int ConnectorConnectionReconnected = 10;
	public const int ConnectorConnectionReconnectedWithNewId = 11;
	public const int ConnectorConnectionHandlingRequestDetailed = 12;
	public const int ConnectorConnectionHandlingRequestSimple = 13;
	public const int ConnectorConnectionReceivedTenantConfig = 14;
	public const int ConnectorConnectionLogConnected = 15;
	public const int ConnectorConnectionConnectError = 16;

	public const int DiscoveryDocumentRetryPolicyLogRetry = 17;
	public const int DiscoveryDocumentKeepingDefaults = 18;
	public const int DiscoveryDocumentUsingDelays = 19;

	public const int HubConnectionFactoryCreatingConnection = 20;

	public const int ResponseTransportTransportingResponse = 21;
	public const int ResponseTransportErrorTransportingResponse = 22;
}
