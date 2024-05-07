namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

internal static class LoggingEventIds
{
	public const int RequestServiceStoringRequest = 1;
	public const int RequestServiceErrorStoringRequest = 2;

	public const int ServiceProviderExtensionsApplyingPendingMigrations = 3;
	public const int ServiceProviderExtensionsNoMigrationsPending = 4;
	public const int ServiceProviderExtensionsCannotRollback = 5;
	public const int ServiceProviderExtensionsMigrationNotFound = 6;
	public const int ServiceProviderExtensionsRollingBack = 7;

	public const int StatisticsServiceAddingNewOrigin = 8;
	public const int StatisticsServiceErrorCreatingOrigin = 9;
	public const int StatisticsServiceUpdateLastSeen = 10;
	public const int StatisticsServiceErrorUpdatingOrigin = 11;
	public const int StatisticsServiceSettingShutdownTime = 12;
	public const int StatisticsServiceErrorSettingShutdownTime = 13;
	public const int StatisticsServiceCleanup = 14;
	public const int StatisticsServiceErrorDeletingOrigins = 15;
	public const int StatisticsServiceAddingNewConnection = 16;
	public const int StatisticsServiceErrorCreatingConnection = 17;
	public const int StatisticsServiceUpdateConnectionLastSeenTime = 18;
	public const int StatisticsServiceUpdateConnectionsLastSeenTime = 19;
	public const int StatisticsServiceErrorUpdatingConnections = 20;
	public const int StatisticsServiceSettingDisconnectTime = 21;
	public const int StatisticsServiceErrorSettingDisconnectTime = 22;
	public const int StatisticsServiceConnectionCleanup = 23;
	public const int StatisticsServiceErrorCleaningUpConnections = 24;
}
