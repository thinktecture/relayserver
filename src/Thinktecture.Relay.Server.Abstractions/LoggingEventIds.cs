namespace Thinktecture.Relay.Server;

internal static class LoggingEventIds
{
	public const int ConnectorRegistryRegisteringConnection = 1;
	public const int ConnectorRegistryUnregisteringConnection = 2;
	public const int ConnectorRegistryCouldNotUnregisterConnection = 3;
	public const int ConnectorRegistryUnknownRequestConnection = 4;
	public const int ConnectorRegistryUnknownAcknowledgeConnection = 5;
	public const int ConnectorRegistryDeliveringRequest = 6;
}
