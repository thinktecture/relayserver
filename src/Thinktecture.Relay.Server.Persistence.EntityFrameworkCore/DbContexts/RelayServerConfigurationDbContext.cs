using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts
{
	/// <summary>
	/// Provides EntityFrameworkCore data access.
	/// </summary>
	public class RelayServerConfigurationDbContext : DbContext
	{
		/// <summary>
		/// The tenants that can connect to the server with their connectors.
		/// </summary>
		/// <remarks>Tenants were formerly known als Links in previous RelayServer versions.</remarks>
		public DbSet<Tenant> Tenants { get; set; }

		/// <summary>
		/// The client secrets a connector needs for authentication when connecting to the server.
		/// </summary>
		public DbSet<ClientSecret> ClientSecrets { get; set; }

		/// <inheritdoc />
		public RelayServerConfigurationDbContext(DbContextOptions<RelayServerConfigurationDbContext> options)
			: base(options)
		{
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ConfigureConfigurationEntities();
		}
	}
}
