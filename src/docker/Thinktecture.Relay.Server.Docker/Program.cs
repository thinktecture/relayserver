using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

namespace Thinktecture.Relay.Server.Docker
{
	public class Program
	{
		public static async Task<int> Main(string[] args)
		{
			try
			{
				var host = CreateHostBuilder(args).Build();

				var config = host.Services.GetRequiredService<IConfiguration>();
				if (config.GetValue<bool>("migrate") || config.GetValue<bool>("migrate-only"))
				{
					Log.Information("Applying migrations");
					await host.Services.ApplyMigrations();

					if (config.GetValue<bool>("migrate-only"))
					{
						Log.Information("Only migrations should be executed");
						return 0;
					}
				}

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
	}
}
