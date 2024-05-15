namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

internal static class LoggingEventIds
{
	public const int RequestServiceStoringRequest = 10001;
	public const int RequestServiceErrorStoringRequest = 10002;

	public const int ServiceProviderExtensionsApplyingPendingMigrations = 10101;
	public const int ServiceProviderExtensionsNoMigrationsPending = 10102;
	public const int ServiceProviderExtensionsCannotRollback = 10103;
	public const int ServiceProviderExtensionsMigrationNotFound = 10104;
	public const int ServiceProviderExtensionsRollingBack = 10105;

	public const int StatisticsServiceAddingNewOrigin = 10201;
	public const int StatisticsServiceErrorCreatingOrigin = 10202;
	public const int StatisticsServiceUpdateLastSeen = 10203;
	public const int StatisticsServiceErrorUpdatingOrigin = 10204;
	public const int StatisticsServiceSettingShutdownTime = 10205;
	public const int StatisticsServiceErrorSettingShutdownTime = 10206;
	public const int StatisticsServiceCleanup = 10207;
	public const int StatisticsServiceErrorDeletingOrigins = 10208;
	public const int StatisticsServiceAddingNewConnection = 10209;
	public const int StatisticsServiceErrorCreatingConnection = 10210;
	public const int StatisticsServiceUpdateConnectionLastSeenTime = 10211;
	public const int StatisticsServiceUpdateConnectionsLastSeenTime = 10212;
	public const int StatisticsServiceErrorUpdatingConnections = 10213;
	public const int StatisticsServiceSettingDisconnectTime = 10214;
	public const int StatisticsServiceErrorSettingDisconnectTime = 10215;
	public const int StatisticsServiceConnectionCleanup = 10216;
	public const int StatisticsServiceErrorCleaningUpConnections = 10217;
}
