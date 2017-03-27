using System;
using System.Data.Entity;
using System.IO;
using System.Web.Http;
using System.Web.Http.Cors;
using Autofac;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json.Serialization;
using NLog;
using NLog.Interface;
using Owin;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.Controller.ManagementWeb;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;
using Thinktecture.Relay.Server.SignalR;

namespace Thinktecture.Relay.Server
{
    internal class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<RelayContext, Migrations.Configuration>());

            var container = RegisterServices();
            container.Resolve<IBackendCommunication>(); // ensure that the BUS to rabbitMQ starts up before accepting connections
            app.UseAutofacMiddleware(container);

            app.UseCors(CorsOptions.AllowAll);
            
            UseOAuthSecurity(app, container);

            MapSignalR(app, container);

            UseWebApi(app, container);

            UseFileServer(app, container);
        }

        private static IContainer RegisterServices()
        {
            var builder = new ContainerBuilder();

            builder.RegisterType<OnPremisesConnection>().ExternallyOwned();
            builder.RegisterType<AuthorizationServerProvider>().SingleInstance();
            builder.RegisterType<PasswordHash>().As<IPasswordHash>();

            builder.RegisterType<PostDataTemporaryStore>().As<IPostDataTemporaryStore>().SingleInstance();

            builder.RegisterType<RabbitMqBusFactory>().As<IRabbitMqBusFactory>().SingleInstance();
            builder.RegisterType<RabbitMqBackendCommunication>().As<IBackendCommunication>().SingleInstance();

            builder.RegisterType<OnPremiseConnectorCallbackFactory>().As<IOnPremiseConnectorCallbackFactory>().SingleInstance();
            builder.RegisterType<Configuration.Configuration>().As<IConfiguration>().SingleInstance();

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

            builder.Register(context => new LoggerAdapter(LogManager.GetLogger("Server"))).As<ILogger>().SingleInstance();

            var container = builder.Build();

            RegisterApiControllers(container);

            return container;
        }

        private static void RegisterApiControllers(IContainer container)
        {
            var configuration = container.Resolve<IConfiguration>();

            var builder = new ContainerBuilder();

            if (configuration.EnableManagementWeb)
            {
                builder.RegisterModule<ManagementWebModule>();
            }

            if (configuration.EnableRelaying)
            {
                builder.RegisterModule<RelayingModule>();
            }

            if (configuration.EnableOnPremiseConnections)
            {
                builder.RegisterModule<OnPremiseConnectionsModule>();
            }

            builder.Update(container);
        }

        private static void UseOAuthSecurity(IAppBuilder app, ILifetimeScope container)
        {
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),
                Provider = container.Resolve<AuthorizationServerProvider>()
            });

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());
        }

        private static void MapSignalR(IAppBuilder app, ILifetimeScope container)
        {
            /*
            foreach (var key in app.Properties.Keys)
            {
                Console.WriteLine("Key: {0}", key);
            }
            OwinHttpListener listener = (OwinHttpListener)app.Properties["Microsoft.Owin.Host.HttpListener.OwinHttpListener"];
            int maxPending;
            int maxAccepts;
            if (listener != null)
            {
                listener.GetRequestProcessingLimits(out maxAccepts, out maxPending);
                Console.WriteLine("Limits: a: {0}, p:{1}", maxAccepts, maxPending);
                listener.SetRequestQueueLimit(5000);
                listener.SetRequestProcessingLimits(1000, maxPending);
                listener.GetRequestProcessingLimits(out maxAccepts, out maxPending);
                Console.WriteLine("Limits: a: {0}, p:{1}", maxAccepts, maxPending);
            }
            else
            {
                Console.WriteLine("No Owin listener found");
            }
            */
            var config = container.Resolve<IConfiguration>();

            if (!config.EnableOnPremiseConnections)
            {
                return;
            }

            GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(config.ConnectionTimeout);
            GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(config.DisconnectTimeout);

            app.MapSignalR<OnPremisesConnection>("/signalr", new ConnectionConfiguration
            {
                Resolver = new AutofacDependencyResolver(container)
            });
        }

        private static void UseWebApi(IAppBuilder app, ILifetimeScope container)
        {
            var configuration = container.Resolve<IConfiguration>();
            var logger = container.Resolve<ILogger>();

            var httpConfig = new HttpConfiguration
            {
                IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
                DependencyResolver = new AutofacWebApiDependencyResolver(container)
            };

            httpConfig.EnableCors(new EnableCorsAttribute("*", "*", "*"));

            httpConfig.SuppressDefaultHostAuthentication();
            httpConfig.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

            if (configuration.EnableRelaying) {  
                logger.Info("Relaying enabled");
                httpConfig.Routes.MapHttpRoute("ClientRequest", "relay/{*path}", new { controller = "Client", action = "Relay" });
            }

            if (configuration.EnableOnPremiseConnections)
            {
                logger.Info("OnPremise Connections enabled ");
                httpConfig.Routes.MapHttpRoute("OnPremiseTargetResponse", "forward", new { controller = "Response", action = "Forward" });
                httpConfig.Routes.MapHttpRoute("OnPremiseTargetRequest", "request/{requestId}", new { controller = "Request", action = "Get" });
            }

            if (configuration.EnableManagementWeb) {
                logger.Info("Management Web enabled");
                httpConfig.Routes.MapHttpRoute("ManagementWeb", "api/managementweb/{controller}/{action}");
            }

            httpConfig.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

            app.UseWebApi(httpConfig);
        }

        private static void UseFileServer(IAppBuilder app, ILifetimeScope container)
        {
            var configuration = container.Resolve<IConfiguration>();

            if (!configuration.EnableManagementWeb) { 
                return;
            }

            try
            {
                var options = new FileServerOptions
                {
                    FileSystem = new PhysicalFileSystem("ManagementWeb"),
                    RequestPath = new PathString("/managementweb"),
                };
                options.DefaultFilesOptions.DefaultFileNames.Add("index.html");

                app.UseFileServer(options);
            }
            catch (DirectoryNotFoundException)
            {
                // no admin web deployed - catch silently
            }
        }
    }
}
