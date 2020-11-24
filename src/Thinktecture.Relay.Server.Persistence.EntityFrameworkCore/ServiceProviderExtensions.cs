using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <summary>
	/// Extension methods for the <see cref="IServiceProvider"/>.
	/// </summary>
	public static class ServiceProviderExtensions
	{
		/// <summary>
		/// Apply the most-recent database migrations.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public static async Task ApplyMigrations(this IServiceProvider serviceProvider)
		{
			using var scope = serviceProvider.CreateScope();
			await using var dbContext = scope.ServiceProvider.GetRequiredService<RelayDbContext>();
			await dbContext.Database.MigrateAsync();
		}
	}
}
