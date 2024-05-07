namespace Thinktecture.Relay.Server.Protocols.SignalR;

internal static class LoggingEventIds
{
	public const int ConnectorHubErrorNoTenantName = 1;
	public const int ConnectorHubIncomingConnectionCreatedTenant = 2;
	public const int ConnectorHubRejectingUnknownTenant = 3;
	public const int ConnectorHubIncomingConnectionUpdatedTenant = 4;
	public const int ConnectorHubIncomingConnection = 5;
	public const int ConnectorHubDisconnectedError = 6;
	public const int ConnectorHubDisconnected = 7;
	public const int ConnectorHubReceivedResponse = 8;
	public const int ConnectorHubReceivedAcknowledge = 9;

	public const int ConnectorTransportTransportingRequest = 10;
	public const int ConnectorTransportErrorTransportingRequest = 11;
}
