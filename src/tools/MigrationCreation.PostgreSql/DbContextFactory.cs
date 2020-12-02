using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.MigrationCreation.PostgreSql
{
	public class DbContextFactory : IDesignTimeDbContextFactory<RelayDbContext>
	{
		public RelayDbContext CreateDbContext(string[] args)
		{
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<RelayDbContext>();

			builder.UseNpgsql(configuration.GetConnectionString("PostgreSql"),
				optionsBuilder => optionsBuilder.MigrationsAssembly("Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql"));

			return new RelayDbContext(builder.Options);
		}
	}
}
