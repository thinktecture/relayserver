using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.InterceptorSample
{
	public class Program
	{
		static async Task<int> Main(string[] args)
		{
			try
			{
				Log.Logger = new LoggerConfiguration()
					.MinimumLevel.Verbose()
					.WriteTo.Console()
					.WriteTo.Seq("http://localhost:5341")
					.CreateLogger();

				Log.Logger.Information("Starting interceptor sample.");

				// Config
				var configBuilder = new ConfigurationBuilder();
				configBuilder.AddJsonFile("appsettings.json", true, false);
				var config = configBuilder.Build();

				// DI
				var services = new ServiceCollection();
				services.AddSingleton<ILogger>(Log.Logger);
				services.AddOnPremiseConnectorServices();
				var serviceProvider = services.BuildServiceProvider();

				// Connector
				var connector = new RelayServerConnector(typeof(Program).Assembly, config["RelayServer:User"], config["RelayServer:Password"], new Uri(config["RelayServer:Url"]), 30, serviceProvider);
				connector.RegisterOnPremiseTarget("lh", new Uri("http://localhost:5000"), false);
		
				await connector.ConnectAsync();

				// Main loop
				bool cancelled = false;
				connector.Disconnected += (s, e) => cancelled = true;
				Console.CancelKeyPress += (s, e) => cancelled = true;

				while (!cancelled)
				{
					await Task.Delay(150);
				}
			}
			catch (Exception ex)
			{
				Log.Logger.Fatal(ex, "A fatal error occured.");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}

			Log.Logger.Information("Interceptor sample quit.");
			return 0;
		}
	}
}
