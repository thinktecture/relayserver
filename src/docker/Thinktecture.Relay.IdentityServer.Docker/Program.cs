using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Serilog;
using Thinktecture.Relay.Docker;

namespace Thinktecture.Relay.IdentityServer.Docker
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
				Log.Fatal(ex, "A fatal error cause service crash.");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}

			return 0;
		}

		public static IHostBuilder CreateHostBuilder(string[] args) => DockerUtils.CreateHostBuilder<Startup>("IdentityServer", args);
	}
}
