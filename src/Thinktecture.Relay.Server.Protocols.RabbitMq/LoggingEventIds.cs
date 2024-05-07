namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal static class LoggingEventIds
{
	public const int ModelExtensionsConsumingConsumer = 1;
	public const int ModelExtensionsLostConsumer = 2;
	public const int ModelExtensionsRestoredConsumer = 3;

	public const int ModelFactoryConnectionRecovered = 4;
	public const int ModelFactoryConnectionClosed = 5;
	public const int ModelFactoryModelCreated = 6;
	public const int ModelFactoryModelCallbackError = 7;
	public const int ModelFactoryModelClosed = 8;

	public const int ServerTransportDispatchingAcknowledge = 9;
	public const int ServerTransportDispatchedResponse = 10;
	public const int ServerTransportDispatchedAcknowledge = 11;
	public const int ServerTransportResponseConsumed = 12;
	public const int ServerTransportAcknowledgeConsumed = 13;

	public const int TenantHandlerAcknowledge = 14;
	public const int TenantHandlerCouldNotParseAcknowledge = 15;
	public const int TenantHandlerReceivedRequest = 16;

	public const int TenantTransportPublishedRequest = 17;
	public const int TenantTransportErrorDispatchingRequest = 18;
}
