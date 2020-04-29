using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer
{
	/// <summary>
	/// Provides extensions methods for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the <see cref="RelayServerConfigurationDbContext"/> to the <see cref="IServiceCollection"/>, preconfigured for SQL Server.
		/// </summary>
		/// <param name="serviceCollection">The <see cref="ServiceCollection"/> to add the context to.</param>
		/// <param name="connectionString">The connection string to use for the db context.</param>
		/// <param name="optionsAction">An optional action to configure the <see cref="SqlServerDbContextOptionsBuilder"/></param>.
		/// <param name="addDefaultMigrations">If the default migrations should be added. Set to false if you want to provide your own migrations.</param>
		/// <param name="contextLifetime"> The lifetime with which to register the DbContext service in the container. </param>
		/// <param name="optionsLifetime"> The lifetime with which to register the DbContextOptions service in the container. </param>
		/// <returns>
		///     The same service collection so that multiple calls can be chained.
		/// </returns>
		public static IServiceCollection AddSqlServerRelayServerConfigurationDbContext(this IServiceCollection serviceCollection,
			string connectionString,
			Action<SqlServerDbContextOptionsBuilder> optionsAction = null,
			bool addDefaultMigrations = true,
			ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
			ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
		{
			return serviceCollection.AddDbContext<RelayServerConfigurationDbContext>(c =>
				{
					c.UseSqlServer(connectionString, o =>
					{
						if (addDefaultMigrations)
						{
							o.MigrationsAssembly(typeof(ServiceCollectionExtensions).Assembly.GetName().Name);
						}

						optionsAction?.Invoke(o);
					});
				},
				contextLifetime,
				optionsLifetime
			)
			.AddRelayServerEntityFrameworkCoreRepositories();
		}
	}
}
