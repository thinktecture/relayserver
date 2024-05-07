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
public static partial class ServiceProviderExtensions
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
			Log.ApplyingPendingMigrations(logger, pending.Length);
			await context.Database.MigrateAsync(cancellationToken);
		}
		else
		{
			Log.NoMigrationsPending(logger);
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
				Log.CannotRollback(logger);
			}
			else
			{
				Log.MigrationNotFound(logger, targetMigration, string.Join("\", \"", applied));
			}
		}
		else
		{
			Log.RollingBack(logger, migration);
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
