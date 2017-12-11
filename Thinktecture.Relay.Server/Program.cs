using System;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Interceptor;
using Topshelf;
using Topshelf.Autofac;

namespace Thinktecture.Relay.Server
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
					var programScope = BuildProgramScope();
					var relayServerScope = BuildRelayServerScope(programScope);

					config.UseAutofacContainer(relayServerScope);
					config.Service<RelayService>(settings =>
					{
						settings.ConstructUsingAutofacContainer();
						settings.WhenStarted(s => s.Start());
						settings.WhenStopped(s =>
						{
							s.Stop();
							relayServerScope.Dispose();
							programScope.Dispose();
						});
					});

					config.RunAsNetworkService();
					config.SetDescription("Thinktecture Relay Server Process");
					config.SetDisplayName("Thinktecture Relay Server");
					config.SetServiceName("TTRelayServer");
					config.ApplyCommandLine();
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
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// ReSharper disable once LocalizableElement
				Console.WriteLine("\nPress any key to close application window...");
				Console.ReadKey(true);
			}
#endif
		}

		private static ILifetimeScope BuildProgramScope()
		{
			var builder = new ContainerBuilder();

			builder.RegisterLogger();
			builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();
			builder.RegisterType<InterceptorLoader>().As<IInterceptorLoader>().SingleInstance();
			builder.RegisterType<RelayServerModule>();

			return builder.Build();
		}

		private static ILifetimeScope BuildRelayServerScope(ILifetimeScope programScope)
		{
			return programScope.BeginLifetimeScope(builder => builder.RegisterModule(programScope.Resolve<RelayServerModule>()));
		}
	}
}
