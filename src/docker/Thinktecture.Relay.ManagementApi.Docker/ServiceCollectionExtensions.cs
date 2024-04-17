using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.ManagementApi.Docker;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddRelayServerDbContext(this IServiceCollection services,
		IConfiguration configuration)
	{
		if ("SqlServer".Equals(configuration.GetValue<string>("DatabaseType"),
			    StringComparison.InvariantCultureIgnoreCase))
			return Server.Persistence.EntityFrameworkCore.SqlServer.ServiceCollectionExtensions
				.AddRelayServerDbContext(services, configuration.GetConnectionString("SqlServer")
					?? throw new InvalidOperationException("No 'SqlServer' connection string found."));

		return Server.Persistence.EntityFrameworkCore.PostgreSql.ServiceCollectionExtensions
				.AddRelayServerDbContext(services, configuration.GetConnectionString("PostgreSql")
					?? throw new InvalidOperationException("No 'PostgreSql' connection string found."));
	}
}
