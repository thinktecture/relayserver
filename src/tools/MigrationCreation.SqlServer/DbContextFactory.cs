using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;
using ServiceCollectionExtensions =
	Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.SqlServer.ServiceCollectionExtensions;

namespace MigrationCreation.SqlServer;

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

		builder.UseSqlServer(configuration.GetConnectionString("SqlServer"),
			optionsBuilder => optionsBuilder.MigrationsAssembly(assemblyName));

		return new RelayDbContext(builder.Options);
	}
}
