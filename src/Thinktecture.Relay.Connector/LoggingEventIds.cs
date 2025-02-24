namespace Thinktecture.Relay.Connector;

internal static class LoggingEventIds
{
	public const int AccessTokenProviderRequestingAccessToken = 10001;

	public const int RelayConnectorPostConfigureOptionsGotDiscoveryDocument = 10101;
	public const int RelayConnectorPostConfigureOptionsErrorRetrievingDiscoveryDocument = 10102;
	public const int RelayConnectorPostConfigureOptionsErrorTargetTypeNotFound = 10103;
	public const int RelayConnectorPostConfigureOptionsCouldNotParseTimeout = 10104;

	public const int ClientRequestHandlerAcknowledgeRequest = 10201;
	public const int ClientRequestHandlerErrorHandlingRequest = 10202;
	public const int ClientRequestHandlerDeliverResponse = 10203;
	public const int ClientRequestHandlerDiscardResponse = 10204;

	public const int ClientRequestWorkerNoTargetFound = 10301;
	public const int ClientRequestWorkerFoundTarget = 10302;
	public const int ClientRequestWorkerRequestingBody = 10303;
	public const int ClientRequestWorkerRequestingTarget = 10304;
	public const int ClientRequestWorkerOutsourcingUnknownBody = 10305;
	public const int ClientRequestWorkerOutsourcingBody = 10306;
	public const int ClientRequestWorkerOutsourcedBody = 10307;
	public const int ClientRequestWorkerOutsourcingBodyFailed = 10308;
	public const int ClientRequestWorkerErrorOutsourcingBody = 10309;
	public const int ClientRequestWorkerInlineBody = 10310;
	public const int ClientRequestWorkerErrorDownloadingBody = 10311;
	public const int ClientRequestWorkerRequestTimedOut = 10312;
	public const int ClientRequestWorkerErrorProcessingRequest = 10313;
	public const int ClientRequestWorkerUploadingBodyFailed = 10314;
	public const int ClientRequestWorkerErrorUploadingBody = 10315;

	public const int RelayTargetRegistryRegisteredTarget = 10401;
	public const int RelayTargetRegistryUnregisteredTarget = 10402;
	public const int RelayTargetRegistryCouldNotUnregisterTarget = 10403;

	public const int RelayWebTargetRequestingTarget = 10501;
	public const int RelayWebTargetRequestedTarget = 10502;

	public const int ClientCredentialsClientConfigureOptionsSetTokenEndpoint = 10601;
}
