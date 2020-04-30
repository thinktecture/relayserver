using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Thinktecture.Relay.Docker
{
	public static class DockerUtils
	{
		public static IHostBuilder CreateHostBuilder<TStartup>(string applicationName, string[] args) where TStartup : class =>
			Host
				.CreateDefaultBuilder(args)
				.UseSerilog((context, loggerConfiguration) =>
				{
					loggerConfiguration
						.MinimumLevel.Information()
						.Enrich.FromLogContext()
						.Enrich.WithProperty("Application", applicationName)
						.ReadFrom.Configuration(context.Configuration)
						.WriteTo.Console();
				})
				.ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<TStartup>());
	}
}
