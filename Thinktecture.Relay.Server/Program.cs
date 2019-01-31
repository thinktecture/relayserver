using System;
using System.Configuration;
using System.Diagnostics;
using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.DependencyInjection;
using Thinktecture.Relay.Server.Interceptor;
using Topshelf;
using Topshelf.Autofac;
using Configuration = Thinktecture.Relay.Server.Config.Configuration;

namespace Thinktecture.Relay.Server
{
	internal static class Program
	{
		private static int Main(string[] args)
		{
			var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			if (!configuration.HasFile)
			{
				// ReSharper disable LocalizableElement
				Console.WriteLine("ERROR: Cannot find a configuration file.");
				Console.WriteLine($"Please provide a config file at '{configuration.FilePath}'.");
				// ReSharper enable LocalizableElement

				return 1;
			}

			Log.Logger = new LoggerConfiguration().ReadFrom.AppSettings().CreateLogger();

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
						settings.WhenStarted(s =>
						{
							s.Start();
							// Make sure we create the heartbeater at service startup
							relayServerScope.Resolve<IOnPremiseConnectionHeartbeater>();
						});
						settings.WhenStopped(s =>
						{
							s.Stop();
							relayServerScope.Dispose();
							programScope.Dispose();
						});
					});

					config.RunAsNetworkService();
					config.SetDescription("Thinktecture RelayServer Process");
					config.SetDisplayName("Thinktecture RelayServer");
					config.SetServiceName("TTRelayServer");
					config.ApplyCommandLine();
				});
			}
			catch (Exception ex)
			{
				Log.Logger.Fatal(ex, "Service crashed");
				return 1;
			}
			finally
			{
				Log.CloseAndFlush();
			}

#if DEBUG
			if (Debugger.IsAttached)
			{
				// ReSharper disable once LocalizableElement
				Console.WriteLine("\nPress any key to close application window...");
				Console.ReadKey(true);
			}
#endif

			return 0;
		}

		private static ILifetimeScope BuildProgramScope()
		{
			var builder = new ContainerBuilder();

			builder.RegisterLogger();

			builder.RegisterType<Configuration>().As<IConfiguration>().SingleInstance();

			builder.RegisterType<CustomCodeAssemblyLoader>().As<ICustomCodeAssemblyLoader>().SingleInstance();
			builder.RegisterType<ControllerLoader>().As<IControllerLoader>().SingleInstance();
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
