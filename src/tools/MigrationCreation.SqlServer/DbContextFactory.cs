using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.MigrationCreation.SqlServer
{
	public class DbContextFactory : IDesignTimeDbContextFactory<RelayServerConfigurationDbContext>
	{
		public RelayServerConfigurationDbContext CreateDbContext(string[] args)
		{
			IConfigurationRoot configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.Build();

			var builder = new DbContextOptionsBuilder<RelayServerConfigurationDbContext>();

			builder.UseSqlServer(configuration.GetConnectionString("SqlServer"),
				optionsBuilder => optionsBuilder.MigrationsAssembly("Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer"));

			return new RelayServerConfigurationDbContext(builder.Options);
		}
	}
}
