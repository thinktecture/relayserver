using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

public static partial class ServiceProviderExtensions
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ServiceProviderExtensionsApplyingPendingMigrations, LogLevel.Information,
			"Applying {MigrationCount} pending migration(s)")]
		public static partial void ApplyingPendingMigrations(ILogger logger, int migrationCount);

		[LoggerMessage(LoggingEventIds.ServiceProviderExtensionsNoMigrationsPending, LogLevel.Debug,
			"No migrations pending")]
		public static partial void NoMigrationsPending(ILogger logger);

		[LoggerMessage(LoggingEventIds.ServiceProviderExtensionsCannotRollback, LogLevel.Error,
			"Cannot rollback any migrations on a non-migrated database")]
		public static partial void CannotRollback(ILogger logger);

		[LoggerMessage(LoggingEventIds.ServiceProviderExtensionsMigrationNotFound, LogLevel.Warning,
			"The provided target migration \"{TargetMigration}\" was not found in the already applied migrations (\"{Migrations}\")")]
		public static partial void MigrationNotFound(ILogger logger, string targetMigration, string migrations);

		[LoggerMessage(LoggingEventIds.ServiceProviderExtensionsRollingBack, LogLevel.Warning,
			"Rolling back to migration {TargetMigration}")]
		public static partial void RollingBack(ILogger logger, string targetMigration);
	}
}
