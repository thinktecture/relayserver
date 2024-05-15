namespace Thinktecture.Relay.Server.Protocols.SignalR;

internal static class LoggingEventIds
{
	public const int ConnectorHubErrorNoTenantName = 10001;
	public const int ConnectorHubIncomingConnectionCreatedTenant = 10002;
	public const int ConnectorHubRejectingUnknownTenant = 10003;
	public const int ConnectorHubIncomingConnectionUpdatedTenant = 10004;
	public const int ConnectorHubIncomingConnection = 10005;
	public const int ConnectorHubDisconnectedError = 10006;
	public const int ConnectorHubDisconnected = 10007;
	public const int ConnectorHubReceivedResponse = 10008;
	public const int ConnectorHubReceivedAcknowledge = 10009;

	public const int ConnectorTransportTransportingRequest = 10101;
	public const int ConnectorTransportErrorTransportingRequest = 10102;
}
