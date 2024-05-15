namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal static class LoggingEventIds
{
	public const int ModelExtensionsConsumingConsumer = 10001;
	public const int ModelExtensionsLostConsumer = 10002;
	public const int ModelExtensionsRestoredConsumer = 10003;

	public const int ModelFactoryConnectionRecovered = 10101;
	public const int ModelFactoryConnectionClosed = 10102;
	public const int ModelFactoryModelCreated = 10103;
	public const int ModelFactoryModelCallbackError = 10104;
	public const int ModelFactoryModelClosed = 10105;

	public const int ServerTransportDispatchingAcknowledge = 10201;
	public const int ServerTransportDispatchedResponse = 10202;
	public const int ServerTransportDispatchedAcknowledge = 10203;
	public const int ServerTransportResponseConsumed = 10204;
	public const int ServerTransportAcknowledgeConsumed = 10205;

	public const int TenantHandlerAcknowledge = 10301;
	public const int TenantHandlerCouldNotParseAcknowledge = 10302;
	public const int TenantHandlerReceivedRequest = 10303;

	public const int TenantTransportPublishedRequest = 10401;
	public const int TenantTransportErrorDispatchingRequest = 10402;
}
