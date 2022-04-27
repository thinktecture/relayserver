using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;

namespace Thinktecture.Relay.StatisticsApi.Docker
{
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
				Log.CloseAndFlush();
			}

			return 0;
		}

		public static IHostBuilder CreateHostBuilder(string[] args) => DockerUtils.CreateHostBuilder<Startup>("StatisticsApi", args);
	}
}