using System;
using System.Diagnostics;
using Serilog;
using Topshelf;

namespace Thinktecture.Relay.OnPremiseConnectorService
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			Log.Logger = new LoggerConfiguration()
				.ReadFrom.AppSettings()
				.CreateLogger();

			try
			{
				HostFactory.Run(config =>
				{
					config.UseSerilog();
					config.Service<OnPremisesService>(settings =>
					{
						settings.ConstructUsing(_ => new OnPremisesService());
						settings.WhenStarted(async s => await s.StartAsync().ConfigureAwait(false));
						settings.WhenStopped(s => s.Stop());
					});
					config.RunAsNetworkService();

					config.SetDescription("Thinktecture Relay OnPremises Service");
					config.SetDisplayName("Thinktecture Relay OnPremises Service");
					config.SetServiceName("TTRelayOnPremisesService");
				});
			}
			catch (Exception ex)
			{
				Log.Logger.Fatal(ex, "Service crashed");
			}
			finally
			{
				Log.CloseAndFlush();
			}

			Log.CloseAndFlush();

#if DEBUG
			if (Debugger.IsAttached)
			{
				// ReSharper disable once LocalizableElement
				Console.WriteLine("\nPress any key to close application window...");
				Console.ReadKey(true);
			}
#endif
		}
	}
}
