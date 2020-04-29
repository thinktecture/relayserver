using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.Entities;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts
{
	/// <summary>
	/// Provides EntityFrameworkCore data access logic for the RelayServer.
	/// </summary>
	public class RelayServerConfigurationDbContext : DbContext
	{
		/// <summary>
		/// The tenants that can connect to the RelayServer with their connectors.
		/// </summary>
		/// <remarks>Tenants were formerly known als Links in previous RelayServer versions.</remarks>
		public DbSet<Tenant> Tenants { get; set; }

		/// <summary>
		/// The client secrets a connector needs for authentication when connecting to the RelayServer.
		/// </summary>
		public DbSet<ClientSecret> ClientSecrets { get; set; }

		/// <summary>
		/// Initializes a new instance of the context.
		/// </summary>
		/// <param name="options"></param>
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
