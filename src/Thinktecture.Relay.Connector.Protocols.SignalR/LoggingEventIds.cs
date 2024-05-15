namespace Thinktecture.Relay.Connector.Protocols.SignalR;

internal static class LoggingEventIds
{
	public const int AcknowledgeTransportTransportingAck = 10001;
	public const int AcknowledgeTransportErrorTransportingAck = 10002;

	public const int ConnectorConnectionDisconnecting = 10101;
	public const int ConnectorConnectionDisconnected = 10102;
	public const int ConnectorConnectionGracefullyClosed = 10103;
	public const int ConnectorConnectionReconnectingAfterLoss = 10104;
	public const int ConnectorConnectionReconnectingAfterError = 10105;
	public const int ConnectorConnectionReconnectedWithoutId = 10106;
	public const int ConnectorConnectionReconnected = 10107;
	public const int ConnectorConnectionReconnectedWithNewId = 10108;
	public const int ConnectorConnectionHandlingRequestDetailed = 10109;
	public const int ConnectorConnectionHandlingRequestSimple = 10110;
	public const int ConnectorConnectionReceivedTenantConfig = 10111;
	public const int ConnectorConnectionLogConnected = 10112;
	public const int ConnectorConnectionConnectError = 10113;

	public const int DiscoveryDocumentRetryPolicyLogRetry = 10201;
	public const int DiscoveryDocumentKeepingDefaults = 10202;
	public const int DiscoveryDocumentUsingDelays = 10203;

	public const int HubConnectionFactoryCreatingConnection = 10301;

	public const int ResponseTransportTransportingResponse = 10401;
	public const int ResponseTransportErrorTransportingResponse = 10402;
}
