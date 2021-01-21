using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the EntityFrameworkCore repositories to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="services">The <see cref="IServiceCollection"/>.</param>
		/// <returns>The <see cref="IServiceCollection"/>.</returns>
		public static IServiceCollection AddRelayServerEntityFrameworkCoreRepositories(this IServiceCollection services)
		{
			services.TryAddScoped<ITenantRepository, TenantRepository>();
			services.TryAddScoped<IStatisticsRepository, StatisticsRepository>();
			services.TryAddScoped<IConnectionRepository, ConnectionRepository>();
			services.TryAddScoped<IRequestRepository, RequestRepository>();

			services.AddHealthChecks()
				.AddDbContextCheck<RelayDbContext>();

			return services;
		}
	}
}
