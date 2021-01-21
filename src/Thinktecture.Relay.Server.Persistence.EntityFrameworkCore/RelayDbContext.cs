using Microsoft.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore
{
	/// <summary>
	/// Provides EntityFrameworkCore data access.
	/// </summary>
	public class RelayDbContext : DbContext
	{
		/// <summary>
		/// The tenants that can connect to the server with their connectors.
		/// </summary>
		/// <remarks>Tenants were formerly known als Links in previous versions.</remarks>
		public DbSet<Tenant> Tenants { get; set; }

		/// <summary>
		/// The client secrets a connector needs for authentication when connecting to the server.
		/// </summary>
		public DbSet<ClientSecret> ClientSecrets { get; set; }

		/// <summary>
		/// The relay server instances.
		/// </summary>
		public DbSet<Origin> Origins { get; set; }

		/// <summary>
		/// The connections.
		/// </summary>
		public DbSet<Connection> Connections { get; set; }

		/// <summary>
		/// The tenant configs.
		/// </summary>
		public DbSet<Config> Configs { get; set; }

		/// <summary>
		/// The requests.
		/// </summary>
		public DbSet<Request> Requests { get; set; }

		/// <inheritdoc />
		public RelayDbContext(DbContextOptions<RelayDbContext> options)
			: base(options)
		{
		}

		/// <inheritdoc />
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.ConfigureEntities();
		}
	}
}
