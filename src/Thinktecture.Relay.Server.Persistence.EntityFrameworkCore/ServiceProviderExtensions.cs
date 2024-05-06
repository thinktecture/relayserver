using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

/// <summary>
/// Extension methods for the <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceProviderExtensions
{
	/// <summary>
	/// Apply the pending database migrations.
	/// </summary>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public static async Task ApplyPendingMigrationsAsync(this IServiceProvider serviceProvider,
		CancellationToken cancellationToken = default)
	{
		using var scope = GetDbContext(serviceProvider, out var context);
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<RelayDbContext>>();

		var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToArray();
		if (pending.Length > 0)
		{
			logger.LogInformation(23200, "Applying {MigrationCount} pending migration(s)", pending.Length);
			await context.Database.MigrateAsync(cancellationToken);
		}
		else
		{
			logger.LogDebug(23201, "No migrations pending");
		}
	}

	/// <summary>
	/// Rolls back the database to a specific migration.
	/// </summary>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
	/// <param name="targetMigration">The name of the migration to return to.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public static async Task RollbackMigrationsAsync(this IServiceProvider serviceProvider, string targetMigration,
		CancellationToken cancellationToken = default)
	{
		using var scope = GetDbContext(serviceProvider, out var context);
		var logger = scope.ServiceProvider.GetRequiredService<ILogger<RelayDbContext>>();

		var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken)).ToArray();
		var migration = applied.FirstOrDefault(m => m.Equals(targetMigration, StringComparison.OrdinalIgnoreCase));
		if (migration is null)
		{
			if (applied.Length == 0)
			{
				logger.LogError(23202, "Cannot rollback any migrations on a non-migrated database");
			}
			else
			{
				logger.LogWarning(23203,
					"The provided target migration \"{TargetMigration}\" was not found in the already applied migrations (\"{Migrations}\")",
					targetMigration, string.Join("\", \"", applied));
			}
		}
		else
		{
			logger.LogWarning(203204, "Rolling back to migration {TargetMigration}", migration);
			await context.GetService<IMigrator>().MigrateAsync(migration, cancellationToken);
		}
	}

	private static IServiceScope GetDbContext(IServiceProvider serviceProvider, out RelayDbContext context)
	{
		var scope = serviceProvider.CreateScope();
		context = scope.ServiceProvider.GetRequiredService<RelayDbContext>();
		return scope;
	}
}
