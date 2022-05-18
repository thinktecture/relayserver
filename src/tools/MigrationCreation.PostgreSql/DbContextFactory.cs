using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;
using ServiceCollectionExtensions =
	Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql.ServiceCollectionExtensions;

namespace MigrationCreation.PostgreSql;

public class DbContextFactory : IDesignTimeDbContextFactory<RelayDbContext>
{
	public RelayDbContext CreateDbContext(string[] args)
	{
		var configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.Build();

		var builder = new DbContextOptionsBuilder<RelayDbContext>();

		var assemblyName = typeof(ServiceCollectionExtensions)
			.GetAssemblySimpleName();

		builder.UseNpgsql(configuration.GetConnectionString("PostgreSql"),
			optionsBuilder => optionsBuilder.MigrationsAssembly(assemblyName));

		return new RelayDbContext(builder.Options);
	}
}
