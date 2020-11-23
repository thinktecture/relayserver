using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.DbContexts;

namespace Thinktecture.Relay.Server.Docker
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			try
			{
				var host = CreateHostBuilder(args).Build();

				await ApplyMigrationsAsync(host);

				await host.RunAsync();
			}
			catch (Exception ex)
			{
				Console.WriteLine("A fatal error cause service crash: {0}", ex);
				Log.Fatal(ex, "A fatal error cause service crash");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}

			return 0;
		}

		public static IHostBuilder CreateHostBuilder(string[] args) => DockerUtils.CreateHostBuilder<Startup>("RelayServer", args);

		private static async Task ApplyMigrationsAsync(IHost host)
		{
			using var scope = host.Services.CreateScope();
			await using var dbContext = scope.ServiceProvider.GetRequiredService<RelayDbContext>();
			await dbContext.Database.MigrateAsync();
		}
	}
}
