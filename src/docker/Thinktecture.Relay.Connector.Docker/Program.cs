using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;

namespace Thinktecture.Relay.Connector.Docker;

public class Program
{
	public static async Task<int> Main(string[] args)
	{
		try
		{
			var host = CreateHostBuilder(args).Build();

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
			await Log.CloseAndFlushAsync();
		}

		return 0;
	}

	public static IHostBuilder CreateHostBuilder(string[] args)
		=> Host
			.CreateDefaultBuilder(args)
			.UseConsoleLifetime()
			.UseSystemd()
			.UseWindowsService()
			.UseSerilog((context, loggerConfiguration) =>
			{
				loggerConfiguration
					.MinimumLevel.Information()
					.Destructure.With<StreamDestructuringPolicy>()
					.Enrich.FromLogContext()
					.Enrich.WithProperty("Application", "Connector")
					.ReadFrom.Configuration(context.Configuration)
					.WriteTo.Console();
			})
			.ConfigureServices(Startup.ConfigureServices);
}
