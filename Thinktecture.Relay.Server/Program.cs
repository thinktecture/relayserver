using System;
using System.Configuration;
using Autofac;
using RabbitMQ.Client;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Logging;
using Thinktecture.Relay.Server.Plugins;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;
using Thinktecture.Relay.Server.SignalR;
using Topshelf;

namespace Thinktecture.Relay.Server
{
	internal static class Program
	{
		private static void Main(string[] args)
		{
			// TODO: Use configuration (see RELAY-68)
			if (!Int32.TryParse(ConfigurationManager.AppSettings["Port"], out var port))
			{
				port = 20000;
			}

			var hostName = ConfigurationManager.AppSettings["HostName"] ?? "+";
			var allowHttp = String.Equals(ConfigurationManager.AppSettings["UseInsecureHttp"], "true", StringComparison.OrdinalIgnoreCase);

			using (var container = BuildContainer())
			using (var scope = RegisterAdditionalDependencies(container))
			{
				HostFactory.Run(config =>
				{
					config.Service<RelayService>(settings =>
					{
						// ReSharper disable once AccessToDisposedClosure, Passt scho ;)
						settings.ConstructUsing(_ => new RelayService(hostName, port, allowHttp, scope));
						settings.WhenStarted(s => s.Start());
						settings.WhenStopped(s => s.Stop());
					});

					config.RunAsNetworkService();
					config.SetDescription("Thinktecture Relay Server Process");
					config.SetDisplayName("Thinktecture Relay Server");
					config.SetServiceName("TTRelayServer");
					config.ApplyCommandLine();
				});
			}

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				// ReSharper disable once LocalizableElement
				Console.WriteLine("\nPress any key to close application window...");
				Console.ReadKey(true);
			}
#endif
		}

		private static IContainer BuildContainer()
		{
			var builder = new ContainerBuilder();

			builder.RegisterModule<LoggingModule>();

			builder.RegisterType<Startup>();
			builder.RegisterType<OnPremisesConnection>().ExternallyOwned();
			builder.RegisterType<AuthorizationServerProvider>().SingleInstance();
			builder.RegisterType<PasswordHash>().As<IPasswordHash>();

			builder.RegisterType<ConnectionFactory>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqFactory>().AsImplementedInterfaces().SingleInstance();
			builder.Register(ctx => ctx.Resolve<IRabbitMqFactory>().CreateConnection()).AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqMessageDispatcher>().AsImplementedInterfaces().SingleInstance();

			builder.RegisterType<BackendCommunication>().AsImplementedInterfaces().SingleInstance()
				.OnActivated(args => args.Instance.Prepare())
				.AutoActivate(); // ensure that the BUS to rabbitMQ starts up before accepting connections

			builder.RegisterType<OnPremiseConnectorCallbackFactory>().As<IOnPremiseConnectorCallbackFactory>().SingleInstance();
			builder.RegisterType<Configuration.Configuration>().As<IConfiguration>().SingleInstance();
			builder.RegisterType<LocalAppDataPersistedSettings>().As<IPersistedSettings>().SingleInstance().AutoActivate(); // fail fast: load setting immediatelly

			builder.RegisterType<LinkRepository>().As<ILinkRepository>().SingleInstance();
			builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();
			builder.RegisterType<LogRepository>().As<ILogRepository>().SingleInstance();
			builder.RegisterType<TraceRepository>().As<ITraceRepository>().SingleInstance();

			builder.RegisterType<RequestLogger>().As<IRequestLogger>().SingleInstance();
			builder.RegisterType<TraceManager>().As<ITraceManager>().SingleInstance();
			builder.RegisterType<TraceFileWriter>().As<ITraceFileWriter>().SingleInstance();
			builder.RegisterType<TraceFileReader>().As<ITraceFileReader>().SingleInstance();
			builder.RegisterType<TraceTransformation>().As<ITraceTransformation>().SingleInstance();

			builder.RegisterType<HttpResponseMessageBuilder>().As<IHttpResponseMessageBuilder>();
			builder.RegisterType<OnPremiseRequestBuilder>().As<IOnPremiseRequestBuilder>();
			builder.RegisterType<PathSplitter>().As<IPathSplitter>();

			builder.RegisterType<PluginLoader>().As<IPluginLoader>();
			builder.RegisterType<PluginManager>().As<IPluginManager>();

			if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryRequestStoragePath"]))
			{
				builder.RegisterType<InMemoryPostDataTemporaryStore>().As<IPostDataTemporaryStore>().SingleInstance();
			}
			else
			{
				builder.RegisterType<FilePostDataTemporaryStore>().As<IPostDataTemporaryStore>().SingleInstance();
			}

			return builder.Build();
		}

		private static ILifetimeScope RegisterAdditionalDependencies(ILifetimeScope scope)
		{
			var pluginLoader = scope.Resolve<IPluginLoader>();

			return scope.BeginLifetimeScope(builder => pluginLoader.LoadPlugins(builder));
		}
	}
}
