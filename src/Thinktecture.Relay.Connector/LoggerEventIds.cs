namespace Thinktecture.Relay.Connector;

internal static class LoggerEventIds
{
	public const int AccessTokenProviderRequestingAccessToken = 1;
	public const int AccessTokenManagementConfigureOptionsGotDiscoveryDocument = 2;
	public const int AccessTokenManagementConfigureOptionsErrorRetrievingDiscoveryDocument = 3;
	public const int AccessTokenManagementConfigureOptionsErrorTargetTypeNotFound = 4;
	public const int AccessTokenManagementConfigureOptionsCouldNotParseTimeout = 5;

	public const int RelayConnectorPostConfigureOptionsGotDiscoveryDocument = 6;

	public const int ClientRequestHandlerAcknowledgeRequest = 7;
	public const int ClientRequestHandlerErrorHandlingRequest = 8;
	public const int ClientRequestHandlerDeliverResponse = 9;
	public const int ClientRequestHandlerDiscardResponse = 10;

	public const int ClientRequestWorkerNoTargetFound = 11;
	public const int ClientRequestWorkerFoundTarget = 12;
	public const int ClientRequestWorkerRequestingBody = 13;
	public const int ClientRequestWorkerRequestingTarget = 14;
	public const int ClientRequestWorkerOutsourcingUnknownBody = 15;
	public const int ClientRequestWorkerOutsourcingBody = 16;
	public const int ClientRequestWorkerOutsourcedBody = 17;
	public const int ClientRequestWorkerOutsourcingBodyFailed = 18;
	public const int ClientRequestWorkerErrorOutsourcingBody = 19;
	public const int ClientRequestWorkerInlineBody = 20;
	public const int ClientRequestWorkerErrorDownloadingBody = 21;
	public const int ClientRequestWorkerRequestTimedOut = 22;
	public const int ClientRequestWorkerErrorProcessingRequest = 23;
	public const int ClientRequestWorkerUploadingBodyFailed = 24;
	public const int ClientRequestWorkerErrorUploadingBody = 25;

	public const int RelayTargetRegistryRegisteredTarget = 26;
	public const int RelayTargetRegistryUnregisteredTarget = 27;
	public const int RelayTargetRegistryCouldNotUnregisterTarget = 28;

	public const int RelayWebTargetRequestingTarget = 29;
	public const int RelayWebTargetRequestedTarget = 30;
}
