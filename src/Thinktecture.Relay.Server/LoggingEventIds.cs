namespace Thinktecture.Relay.Server;

internal static class LoggingEventIds
{
	public const int AcknowledgeControllerAcknowledgementReceived = 1;

	public const int BodyContentControllerDeliverBody = 2;
	public const int BodyContentControllerStoreBody = 3;
	public const int BodyContentControllerResponseAborted = 4;

	public const int DiscoveryDocumentControllerReturnDiscoveryDocument = 5;

	public const int MaintenanceJobRunnerRunMaintenanceJob = 6;
	public const int MaintenanceJobRunnerRunMaintenanceJobFailed = 7;

	public const int RelayMiddlewareInvalidRequest = 8;
	public const int RelayMiddlewareUnknownTenant = 9;
	public const int RelayMiddlewareRequestParsed = 10;
	public const int RelayMiddlewareResponseReceived = 11;
	public const int RelayMiddlewareClientAborted = 12;
	public const int RelayMiddlewareRequestExpired = 13;
	public const int RelayMiddlewareErrorHandlingRequest = 14;
	public const int RelayMiddlewareNoActiveConnection = 15;
	public const int RelayMiddlewareAcknowledgeModeChange = 16;
	public const int RelayMiddlewareDiscardConnectorResponse = 17;
	public const int RelayMiddlewareExecutingRequestInterceptors = 18;
	public const int RelayMiddlewareExecutingRequestInterceptor = 19;
	public const int RelayMiddlewareDeliveringRequest = 20;
	public const int RelayMiddlewareWaitForResponse = 21;
	public const int RelayMiddlewareExecutingResponseInterceptors = 22;
	public const int RelayMiddlewareExecutingResponseInterceptor = 23;
	public const int RelayMiddlewareOutsourcingRequestBody = 24;
	public const int RelayMiddlewareOutsourcedRequestBody = 25;
	public const int RelayMiddlewareInlinedRequestBody = 26;

	public const int RelayTargetResponseWriterFailedRequest = 27;

	public const int AcknowledgeCoordinatorRegisterAcknowledgeState = 28;
	public const int AcknowledgeCoordinatorReRegisterAcknowledgeState = 29;
	public const int AcknowledgeCoordinatorUnknownRequest = 30;
	public const int AcknowledgeCoordinatorPrunedRequest = 31;

	public const int AcknowledgeDispatcherLocalAcknowledge = 32;
	public const int AcknowledgeDispatcherRedirectAcknowledge = 33;

	public const int FileBodyStoreUsingStorage = 34;
	public const int FileBodyStoreOperation = 35;
	public const int FileBodyStoreWriting = 36;
	public const int FileBodyStoreError = 37;

	public const int FileBodyStoreValidateOptionsErrorCheckingPermissions = 38;

	public const int InMemoryBodyStoreUsingStorage = 39;

	public const int InMemoryTenantTransportErrorDeliveringRequest = 40;

	public const int RequestCoordinatorRedirect = 41;

	public const int ResponseCoordinatorRequestAlreadyRegistered = 42;
	public const int ResponseCoordinatorWaitingForResponse = 43;
	public const int ResponseCoordinatorNoWaitingStateFound = 44;
	public const int ResponseCoordinatorCancelingWait = 45;
	public const int ResponseCoordinatorBodyOpened = 46;
	public const int ResponseCoordinatorInlinedReceived = 47;
	public const int ResponseCoordinatorNoBodyReceived = 48;
	public const int ResponseCoordinatorResponseReceived = 49;
	public const int ResponseCoordinatorResponseDiscarded = 50;

	public const int ResponseDispatcherLocalDispatch = 51;
	public const int ResponseDispatcherRedirectDispatch = 52;
}
