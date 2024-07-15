namespace Thinktecture.Relay.Server;

internal static class LoggingEventIds
{
	public const int AcknowledgeEndpointAcknowledgementReceived = 10001;

	public const int BodyContentEndpointDeliverBody = 10101;
	public const int BodyContentEndpointStoreBody = 10102;
	public const int BodyContentEndpointResponseAborted = 10103;

	public const int DiscoveryDocumentEndpointReturnDiscoveryDocument = 10201;

	public const int MaintenanceJobRunnerRunMaintenanceJob = 10301;
	public const int MaintenanceJobRunnerRunMaintenanceJobFailed = 10302;

	public const int RelayMiddlewareInvalidRequest = 10401;
	public const int RelayMiddlewareUnknownTenant = 10402;
	public const int RelayMiddlewareRequestParsed = 10403;
	public const int RelayMiddlewareResponseReceived = 10404;
	public const int RelayMiddlewareClientAborted = 10405;
	public const int RelayMiddlewareRequestExpired = 10406;
	public const int RelayMiddlewareErrorHandlingRequest = 10407;
	public const int RelayMiddlewareNoActiveConnection = 10408;
	public const int RelayMiddlewareAcknowledgeModeChange = 10409;
	public const int RelayMiddlewareDiscardConnectorResponse = 10410;
	public const int RelayMiddlewareExecutingRequestInterceptors = 10411;
	public const int RelayMiddlewareExecutingRequestInterceptor = 10412;
	public const int RelayMiddlewareDeliveringRequest = 10413;
	public const int RelayMiddlewareWaitForResponse = 10414;
	public const int RelayMiddlewareExecutingResponseInterceptors = 10415;
	public const int RelayMiddlewareExecutingResponseInterceptor = 10416;
	public const int RelayMiddlewareOutsourcingRequestBody = 10417;
	public const int RelayMiddlewareOutsourcedRequestBody = 10418;
	public const int RelayMiddlewareInlinedRequestBody = 10419;

	public const int RelayTargetResponseWriterFailedRequest = 10501;

	public const int AcknowledgeCoordinatorRegisterAcknowledgeState = 10601;
	public const int AcknowledgeCoordinatorReRegisterAcknowledgeState = 10602;
	public const int AcknowledgeCoordinatorUnknownRequest = 10603;
	public const int AcknowledgeCoordinatorPrunedRequest = 10604;

	public const int AcknowledgeDispatcherLocalAcknowledge = 10701;
	public const int AcknowledgeDispatcherRedirectAcknowledge = 10702;

	public const int FileBodyStoreUsingStorage = 10801;
	public const int FileBodyStoreOperation = 10802;
	public const int FileBodyStoreWriting = 10803;
	public const int FileBodyStoreError = 10804;

	public const int FileBodyStoreValidateOptionsErrorCheckingPermissions = 10901;

	public const int InMemoryBodyStoreUsingStorage = 11001;

	public const int InMemoryTenantTransportErrorDeliveringRequest = 11101;

	public const int RequestCoordinatorRedirect = 11201;

	public const int ResponseCoordinatorRequestAlreadyRegistered = 11301;
	public const int ResponseCoordinatorWaitingForResponse = 11302;
	public const int ResponseCoordinatorNoWaitingStateFound = 11303;
	public const int ResponseCoordinatorCancelingWait = 11304;
	public const int ResponseCoordinatorBodyOpened = 11305;
	public const int ResponseCoordinatorInlinedReceived = 11306;
	public const int ResponseCoordinatorNoBodyReceived = 11307;
	public const int ResponseCoordinatorResponseReceived = 11308;
	public const int ResponseCoordinatorResponseDiscarded = 11309;

	public const int ResponseDispatcherLocalDispatch = 11401;
	public const int ResponseDispatcherRedirectDispatch = 11402;
}
