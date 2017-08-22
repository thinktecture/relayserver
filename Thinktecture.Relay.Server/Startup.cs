using System;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.ExceptionHandling;
using Autofac;
using Autofac.Integration.SignalR;
using Autofac.Integration.WebApi;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.Security.DataHandler;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.StaticFiles;
using Newtonsoft.Json.Serialization;
using NLog;
using Owin;
using RabbitMQ.Client;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.Controller.ManagementWeb;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Filters;
using Thinktecture.Relay.Server.Helper;
using Thinktecture.Relay.Server.Http;
using Thinktecture.Relay.Server.Logging;
using Thinktecture.Relay.Server.Owin;
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
			var config = container.Resolve<IConfiguration>();
			var logger = container.Resolve<ILogger>();
			var httpConfig = CreateHttpConfiguration(config, logger);

			var scope = container.BeginLifetimeScope(builder =>
			{
				builder.RegisterWebApiFilterProvider(httpConfig);
				RegisterApiControllers(builder, config);
			});

			app.UseAutofacMiddleware(scope);
			app.UseCors(CorsOptions.AllowAll);
			UseOAuthSecurity(app, scope.Resolve<AuthorizationServerProvider>());
			MapSignalR(app, scope, config);
			UseWebApi(app, httpConfig, scope);
			UseFileServer(app, config, logger);
		}

		private static IContainer RegisterServices()
		{
			var builder = new ContainerBuilder();

			builder.RegisterType<OnPremisesConnection>().ExternallyOwned();
			builder.RegisterType<AuthorizationServerProvider>().SingleInstance();
			builder.RegisterType<PasswordHash>().As<IPasswordHash>();

			builder.RegisterType<ConnectionFactory>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqFactory>().AsImplementedInterfaces().SingleInstance();
			builder.Register(ctx => ctx.Resolve<IRabbitMqFactory>().CreateConnection()).AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<RabbitMqMessageDispatcher>().AsImplementedInterfaces().SingleInstance();
			builder.RegisterType<BackendCommunication>().AsImplementedInterfaces().SingleInstance().AutoActivate(); // ensure that the BUS to rabbitMQ starts up before accepting connections

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

			builder.Register(context => LogManager.GetLogger("Server")).As<ILogger>().SingleInstance();

			if (String.IsNullOrWhiteSpace(ConfigurationManager.AppSettings["TemporaryRequestStoragePath"]))
			{
				builder.RegisterType<InMemoryPostDataTemporaryStore>().As<IPostDataTemporaryStore>().SingleInstance();
			}
			else
			{
				builder.RegisterType<FilePostDataTemporaryStore>().As<IPostDataTemporaryStore>().SingleInstance();
			}

			var container = builder.Build();


			return container;
		}

		private static void RegisterApiControllers(ContainerBuilder builder, IConfiguration configuration)
		{
			if (configuration.EnableManagementWeb != ModuleBinding.False)
				builder.RegisterModule<ManagementWebModule>();

			if (configuration.EnableRelaying != ModuleBinding.False)
				builder.RegisterModule<RelayingModule>();

			if (configuration.EnableOnPremiseConnections != ModuleBinding.False)
				builder.RegisterModule<OnPremiseConnectionsModule>();
		}

		private static void UseOAuthSecurity(IAppBuilder app, AuthorizationServerProvider authProvider)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));
			if (authProvider == null)
				throw new ArgumentNullException(nameof(authProvider));

			var serverOptions = new OAuthAuthorizationServerOptions
			{
				AllowInsecureHttp = true,
				TokenEndpointPath = new PathString("/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),
				Provider = authProvider
			};

			var sharedSecret = ConfigurationManager.AppSettings["OAuthSharedSecret"];

			if (!String.IsNullOrWhiteSpace(sharedSecret))
			{
				UseSharedSecret(app, sharedSecret, serverOptions);
				return;
			}

			var certBase64 = ConfigurationManager.AppSettings["OAuthCertificate"];
			var authOptions = new OAuthBearerAuthenticationOptions();

			if (!String.IsNullOrWhiteSpace(certBase64))
			{
				var certRaw = Convert.FromBase64String(certBase64);
				var oauthCert = new X509Certificate2(certRaw);

				serverOptions.AccessTokenFormat = new TicketDataFormat(new RsaDataProtector(oauthCert));
				authOptions.AccessTokenFormat = new TicketDataFormat(new RsaDataProtector(oauthCert));
			}

			authOptions.Provider = new OAuthBearerAuthenticationProvider()
			{
				OnApplyChallenge = context =>
				{
					// Workaround: Keep an already set WWW-Authenticate header (otherwise OWIN would add its challenge). 
					if (!context.Response.Headers.ContainsKey("WWW-Authenticate"))
					{
						context.OwinContext.Response.Headers.AppendValues("WWW-Authenticate", context.Challenge);
					}
					return Task.FromResult(0);
				}
			};

			app.UseOAuthAuthorizationServer(serverOptions);
			app.UseOAuthBearerAuthentication(authOptions);
		}

		private static void UseSharedSecret(IAppBuilder app, string sharedSecret, OAuthAuthorizationServerOptions serverOptions)
		{
			var issuer = "http://thinktecture.com/relayserver/sts";
			var audience = "http://thinktecture.com/relayserver/consumers";
			var key = Convert.FromBase64String(sharedSecret);

			serverOptions.AccessTokenFormat = new CustomJwtFormat(serverOptions.AccessTokenExpireTimeSpan, key, issuer, audience);

			app.UseOAuthAuthorizationServer(serverOptions);
			app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions()
			{
				AllowedAudiences = new[] { audience },
				IssuerSecurityTokenProviders = new[] { new SymmetricKeyIssuerSecurityTokenProvider(issuer, key) }
			});
		}

		private static void MapSignalR(IAppBuilder app, ILifetimeScope scope, IConfiguration config)
		{
			if (config.EnableOnPremiseConnections == ModuleBinding.False)
				return;

			var path = "/signalr";
			GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(config.ConnectionTimeout);
			GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(config.DisconnectTimeout);
			GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(config.KeepAliveInterval);

			if (config.EnableOnPremiseConnections == ModuleBinding.Local)
			{
				app.Use(typeof(BlockNonLocalRequestsMiddleware), path);
			}

			app.MapSignalR<OnPremisesConnection>(path, new ConnectionConfiguration
			{
				Resolver = new AutofacDependencyResolver(scope),
			});
		}

		private static void UseWebApi(IAppBuilder app, HttpConfiguration httpConfig, ILifetimeScope scope)
		{
			httpConfig.DependencyResolver = new AutofacWebApiDependencyResolver(scope);

			app.UseWebApi(httpConfig);
		}

		private static HttpConfiguration CreateHttpConfiguration(IConfiguration configuration, ILogger logger)
		{
			var httpConfig = new HttpConfiguration
			{
				IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always,
			};

			httpConfig.EnableCors(new EnableCorsAttribute("*", "*", "*"));

			httpConfig.SuppressDefaultHostAuthentication();

			var enableNLogTraceWriter = StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableNLogTraceWriter"], "true");
			if (enableNLogTraceWriter)
				httpConfig.Services.Replace(typeof(System.Web.Http.Tracing.ITraceWriter), new NLogTraceWriter(logger, new TraceLevelConverter()));

			httpConfig.Services.Add(typeof(IExceptionLogger), new NLogExceptionLogger(logger));

			var enableNLogActionFilter = StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableNLogActionFilter"], "true");
			if (enableNLogActionFilter)
				httpConfig.Filters.Add(new NLogActionFilter(logger));

			httpConfig.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			if (configuration.EnableRelaying != ModuleBinding.False)
			{
				logger.Info("Relaying enabled");
				httpConfig.Routes.MapHttpRoute("ClientRequest", "relay/{*path}", new { controller = "Client", action = "Relay" });
			}

			if (configuration.EnableOnPremiseConnections != ModuleBinding.False)
			{
				logger.Info("On-premise connections enabled");
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetResponse", "forward", new { controller = "Response", action = "Forward" });
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetRequest", "request/{requestId}", new { controller = "Request", action = "Get" });
			}

			if (configuration.EnableManagementWeb != ModuleBinding.False)
			{
				logger.Info("Management web enabled");
				httpConfig.Routes.MapHttpRoute("ManagementWeb", "api/managementweb/{controller}/{action}");
			}

			httpConfig.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			return httpConfig;
		}

		private static void UseFileServer(IAppBuilder app, IConfiguration configuration, ILogger logger)
		{
			if (configuration.EnableManagementWeb == ModuleBinding.False)
				return;

			try
			{
				var path = "/managementweb";

				var options = new FileServerOptions()
				{
					FileSystem = new PhysicalFileSystem(configuration.ManagementWebLocation),
					RequestPath = new PathString(path),
				};
				options.DefaultFilesOptions.DefaultFileNames.Add("index.html");

				if (configuration.EnableManagementWeb == ModuleBinding.Local)
				{
					app.Use(typeof(BlockNonLocalRequestsMiddleware), path);
				}

				app.UseFileServer(options);
			}
			catch (DirectoryNotFoundException)
			{
				// no admin web deployed - catch silently, but display info for the user
				logger.Info("The configured directory for the ManagementWeb was not found. ManagementWeb will be disabled.");
			}
		}
	}
}
