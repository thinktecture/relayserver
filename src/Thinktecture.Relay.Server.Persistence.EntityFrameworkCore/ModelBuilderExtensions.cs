using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;
using Thinktecture.Relay.Server.Persistence.Models;

// ReSharper disable once CheckNamespace; (extension methods on ModelBuilder namespace)
namespace Microsoft.EntityFrameworkCore
{
	/// <summary>
	/// Extension methods for the <see cref="ModelBuilder"/>.
	/// </summary>
	public static class ModelBuilderExtensions
	{
		/// <summary>
		/// Configures the <see cref="RelayServerConfigurationDbContext"/> entities.
		/// </summary>
		/// <param name="modelBuilder">The <see cref="ModelBuilder"/> to configure.</param>
		/// <returns>The configured instance of the <see cref="ModelBuilder"/>.</returns>
		public static ModelBuilder ConfigureConfigurationEntities(this ModelBuilder modelBuilder)
		{
			return modelBuilder
				.ConfigureTenant()
				.ConfigureClientSecret();
		}

		/// <summary>
		/// Configures the <see cref="Tenant"/> entity.
		/// </summary>
		/// <param name="modelBuilder">The <see cref="ModelBuilder"/> to configure.</param>
		/// <returns>The configured instance of the <see cref="ModelBuilder"/>.</returns>
		public static ModelBuilder ConfigureTenant(this ModelBuilder modelBuilder)
		{
			return modelBuilder.Entity<Tenant>(tenant =>
			{
				tenant
					.HasKey(t => t.Id);

				tenant
					.Property(t => t.Name)
					.HasMaxLength(100)
					.IsRequired();

				tenant
					.HasIndex(t => t.Name)
					.IsUnique();

				tenant
					.Property(t => t.DisplayName)
					.HasMaxLength(200);

				tenant
					.Property(t => t.Description)
					.HasMaxLength(1000);

				tenant.HasMany(t => t.ClientSecrets)
					.WithOne(cs => cs.Tenant)
					.HasForeignKey(cs => cs.TenantId)
					.IsRequired()
					.OnDelete(DeleteBehavior.Cascade);
			});
		}

		/// <summary>
		/// Configures the <see cref="ClientSecret"/> entity.
		/// </summary>
		/// <param name="modelBuilder">The <see cref="ModelBuilder"/> to configure.</param>
		/// <returns>The configured instance of the <see cref="ModelBuilder"/>.</returns>
		public static ModelBuilder ConfigureClientSecret(this ModelBuilder modelBuilder)
		{
			return modelBuilder.Entity<ClientSecret>(clientSecret =>
			{
				clientSecret
					.HasKey(t => t.Id);

				clientSecret
					.Property(t => t.Value)
					.HasMaxLength(4000)
					.IsRequired();
			});
		}
	}
}
