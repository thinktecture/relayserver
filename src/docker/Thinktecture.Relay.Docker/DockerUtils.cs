using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Thinktecture.Relay.Docker
{
	/// <summary>
	/// Provides utility functions for the Docker environment
	/// </summary>
	public static class DockerUtils
	{
		/// <summary>
		/// Creates a host builder preconfigured for the Docker environment.
		/// </summary>
		/// <param name="applicationName">The name of the application.</param>
		/// <param name="args">The command-line arguments provided to the application.</param>
		/// <typeparam name="TStartup">The startup class to use.</typeparam>
		/// <returns>An instance of an <see cref="IHostBuilder"/>.</returns>
		public static IHostBuilder CreateHostBuilder<TStartup>(string applicationName, string[] args)
			where TStartup : class
			=> Host
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
