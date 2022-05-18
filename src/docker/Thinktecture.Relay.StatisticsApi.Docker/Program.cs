using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;

namespace Thinktecture.Relay.StatisticsApi.Docker;

/// <summary>
/// The statistics api program.
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global; Main entry point
public class Program
{
	/// <summary>
	/// The main entry point for the statistics api.
	/// </summary>
	/// <param name="args">The command line arguments.</param>
	/// <returns>A return code for the OS to handle.</returns>
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
			Log.CloseAndFlush();
		}

		return 0;
	}

	/// <summary>
	/// Creates a host builder.
	/// </summary>
	/// <param name="args">The command line args.</param>
	/// <returns>An instance of an <see cref="IHostBuilder"/>.</returns>
	// ReSharper disable once MemberCanBePrivate.Global; Could be used by dotnet cli tools like EF cli.
	public static IHostBuilder CreateHostBuilder(string[] args)
		=> DockerUtils.CreateHostBuilder<Startup>("StatisticsApi", args);
}
