using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;
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
		/// Configures the <see cref="RelayDbContext"/> entities for configuration.
		/// </summary>
		/// <param name="modelBuilder">The <see cref="ModelBuilder"/> to configure.</param>
		/// <returns>The configured instance of the <see cref="ModelBuilder"/>.</returns>
		public static ModelBuilder ConfigureEntities(this ModelBuilder modelBuilder)
		{
			return modelBuilder
				.ConfigureTenant()
				.ConfigureClientSecret()
				.ConfigureOrigin()
				.ConfigureConnection()
				.ConfigureConfig()
				.ConfigureRequest();
		}

		private static ModelBuilder ConfigureTenant(this ModelBuilder modelBuilder) => modelBuilder.Entity<Tenant>(builder =>
		{
			builder
				.HasKey(e => e.Id);

			builder
				.Property(e => e.Name)
				.HasMaxLength(100)
				.IsRequired();

			builder
				.HasIndex(e => e.Name)
				.IsUnique();

			builder
				.Property(e => e.DisplayName)
				.HasMaxLength(200);

			builder
				.Property(e => e.Description)
				.HasMaxLength(1000);

			builder
				.HasMany(e => e.ClientSecrets)
				.WithOne()
				.HasForeignKey(e => e.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.Property(e => e.NormalizedName)
				.HasMaxLength(100)
				.IsRequired();

			builder
				.HasIndex(e => e.NormalizedName)
				.IsUnique();

			builder
				.HasMany(e => e.Connections)
				.WithOne()
				.HasForeignKey(e => e.TenantId)
				.OnDelete(DeleteBehavior.Cascade);

			builder
				.HasMany(e => e.Requests)
				.WithOne()
				.HasForeignKey(e => e.TenantId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		private static ModelBuilder ConfigureClientSecret(this ModelBuilder modelBuilder) => modelBuilder.Entity<ClientSecret>(builder =>
		{
			builder
				.HasKey(e => e.Id);

			builder
				.Property(e => e.Value)
				.HasMaxLength(4000)
				.IsRequired();
		});

		private static ModelBuilder ConfigureOrigin(this ModelBuilder modelBuilder) => modelBuilder.Entity<Origin>(builder =>
		{
			builder
				.HasKey(e => e.Id);

			builder
				.Property(e => e.Id)
				.ValueGeneratedNever();

			builder
				.HasMany(e => e.Connections)
				.WithOne()
				.HasForeignKey(e => e.OriginId)
				.OnDelete(DeleteBehavior.Cascade);
		});

		private static ModelBuilder ConfigureConnection(this ModelBuilder modelBuilder) => modelBuilder.Entity<Connection>(builder =>
		{
			builder
				.HasKey(e => e.Id);

			builder
				.Property(e => e.Id)
				.HasMaxLength(100);
		});

		private static ModelBuilder ConfigureConfig(this ModelBuilder modelBuilder) => modelBuilder.Entity<Config>(builder =>
		{
			builder
				.HasKey(e => e.TenantId);
		});

		private static ModelBuilder ConfigureRequest(this ModelBuilder modelBuilder) => modelBuilder.Entity<Request>(builder =>
		{
			builder
				.Property<int>("Id")
				.ValueGeneratedOnAdd();
			builder.HasKey("Id");

			builder
				.Property(e => e.RequestUrl)
				.HasMaxLength(1000)
				.IsRequired();

			builder
				.Property(e => e.Target)
				.HasMaxLength(100)
				.IsRequired();

			builder
				.Property(e => e.HttpMethod)
				.HasMaxLength(10)
				.IsRequired();
		});
	}
}
