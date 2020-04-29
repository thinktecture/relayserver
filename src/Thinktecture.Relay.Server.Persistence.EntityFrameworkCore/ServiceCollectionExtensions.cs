using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <summary>
	/// Provides extensions methods for the <see cref="IServiceCollection"/>.
	/// </summary>
	public static class ServiceCollectionExtensions
	{
		/// <summary>
		/// Adds the EntityFrameworkCore repositories to the <see cref="IServiceCollection"/>.
		/// </summary>
		/// <param name="serviceCollection">The <see cref="IServiceCollection"/> to add our repositories to.</param>
		/// <returns>The <see cref="IServiceCollection"/>.</returns>
		public static IServiceCollection AddRelayServerEntityFrameworkCoreRepositories(this IServiceCollection serviceCollection)
		{
			serviceCollection.TryAddScoped<ITenantRepository, TenantRepository>();

			return serviceCollection;
		}
	}
}
