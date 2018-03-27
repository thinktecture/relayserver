using System;
using Autofac;
using Microsoft.Owin.Security.OAuth;
using RabbitMQ.Client;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Interceptor;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	internal class RelayServerModule : Module
	{
		private readonly ICustomCodeAssemblyLoader _customCodeAssemblyLoader;
		private readonly IControllerLoader _controllerLoader;
		private readonly IInterceptorLoader _interceptorLoader;
		private readonly IDataStoreLoader _dataStoreLoader;

		public RelayServerModule(ICustomCodeAssemblyLoader customCodeAssemblyLoader, IControllerLoader controllerLoader, IInterceptorLoader interceptorLoader, IDataStoreLoader dataStoreLoader)
		{
			_customCodeAssemblyLoader = customCodeAssemblyLoader ?? throw new ArgumentNullException(nameof(customCodeAssemblyLoader));
			_controllerLoader = controllerLoader ?? throw new ArgumentNullException(nameof(controllerLoader));
			_interceptorLoader = interceptorLoader ?? throw new ArgumentNullException(nameof(interceptorLoader));
			_dataStoreLoader = dataStoreLoader ?? throw new ArgumentNullException(nameof(dataStoreLoader));
		}

		protected override void Load(ContainerBuilder builder)
		{
			base.Load(builder);

			builder.RegisterType<RelayService>().SingleInstance();
			builder.RegisterType<Startup>().As<IStartup>().SingleInstance();
			builder.RegisterType<PasswordHash>().As<IPasswordHash>().SingleInstance();
			builder.RegisterType<LinkRepository>().As<ILinkRepository>().SingleInstance();
			builder.RegisterType<UserRepository>().As<IUserRepository>().SingleInstance();

			builder.RegisterType<OnPremisesConnection>().ExternallyOwned();
			builder.RegisterType<AuthorizationServerProvider>().As<IOAuthAuthorizationServerProvider>().SingleInstance();

			builder.RegisterType<ConnectionFactory>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqFactory>().AsImplementedInterfaces().SingleInstance();
			builder.Register(ctx => ctx.Resolve<IRabbitMqFactory>().CreateConnection()).AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqMessageDispatcher>().AsImplementedInterfaces().SingleInstance();

			builder.RegisterType<BackendCommunication>().AsImplementedInterfaces().SingleInstance()
				.OnActivated(args => args.Instance.Prepare())
				.AutoActivate(); // ensure that the BUS to rabbitMQ starts up before accepting connections

			builder.RegisterType<OnPremiseConnectorCallbackFactory>().As<IOnPremiseConnectorCallbackFactory>().SingleInstance();

			builder.RegisterType<LocalAppDataPersistedSettings>().As<IPersistedSettings>().SingleInstance().AutoActivate(); // fail fast: load setting immediatelly

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

			builder.RegisterType<InterceptorManager>().As<IInterceptorManager>();

			builder.RegisterType<PasswordComplexityValidator>().AsImplementedInterfaces();

			_customCodeAssemblyLoader.RegisterModule(builder);

			_controllerLoader.RegisterControllers(builder);
			_interceptorLoader.RegisterInterceptors(builder);
			_dataStoreLoader.RegisterDataStore(builder);
		}
	}
}
