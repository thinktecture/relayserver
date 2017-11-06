using System;
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

			Log.CloseAndFlush();

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// ReSharper disable once LocalizableElement
				Console.WriteLine("\nPress any key to close application window...");
				Console.ReadKey(true);
			}
#endif
		}
	}
}
