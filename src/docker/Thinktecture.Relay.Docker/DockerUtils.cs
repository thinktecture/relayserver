using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Thinktecture.Relay.Docker
{
	public static class DockerUtils
	{
		public static IHostBuilder CreateHostBuilder<TStartup>(string applicationName, string[] args)
			where TStartup : class =>
			Host
				.CreateDefaultBuilder(args)
				.ConfigureServices((context, services) => services.Configure<KestrelServerOptions>(context.Configuration.GetSection("Kestrel")))
				.UseSerilog((context, loggerConfiguration) =>
				{
					loggerConfiguration
						.MinimumLevel.Information()
						.Destructure.With<StreamDestructuringPolicy>()
						.Enrich.FromLogContext()
						.Enrich.WithProperty("Application", applicationName)
						.ReadFrom.Configuration(context.Configuration)
						.WriteTo.Console();
				})
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());
	}
}
