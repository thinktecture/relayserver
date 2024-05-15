namespace Thinktecture.Relay.Server;

internal static class LoggingEventIds
{
	public const int ConnectorRegistryRegisteringConnection = 10001;
	public const int ConnectorRegistryUnregisteringConnection = 10002;
	public const int ConnectorRegistryCouldNotUnregisterConnection = 10003;
	public const int ConnectorRegistryUnknownRequestConnection = 10004;
	public const int ConnectorRegistryUnknownAcknowledgeConnection = 10005;
	public const int ConnectorRegistryDeliveringRequest = 10006;
}
