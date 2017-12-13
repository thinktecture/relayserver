using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
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
using Owin;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.Controller;
using Thinktecture.Relay.Server.Controller.ManagementWeb;
using Thinktecture.Relay.Server.Filters;
using Thinktecture.Relay.Server.Logging;
using Thinktecture.Relay.Server.Owin;
using Thinktecture.Relay.Server.Repository;
using Thinktecture.Relay.Server.Security;
using Thinktecture.Relay.Server.SignalR;
using ExceptionLogger = Thinktecture.Relay.Server.Logging.ExceptionLogger;

namespace Thinktecture.Relay.Server
{
	internal class Startup : IStartup
	{
		private readonly ILifetimeScope _rootScope;
		private readonly ILogger _logger;
		private readonly IOAuthAuthorizationServerProvider _authorizationServerProvider;
		private readonly IConfiguration _configuration;

		public Startup(ILogger logger, IConfiguration configuration, IOAuthAuthorizationServerProvider authorizationServerProvider, ILifetimeScope rootScope)
		{
			_logger = logger;

			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_authorizationServerProvider = authorizationServerProvider ?? throw new ArgumentNullException(nameof(authorizationServerProvider));
			_rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
		}

		public void Configuration(IAppBuilder app)
		{
			InitializeAndMigrateDatabase();

			var httpConfig = CreateHttpConfiguration(_configuration, _logger);
			var innerScope = RegisterAdditionalServices(_rootScope, httpConfig, _configuration);

			app.UseAutofacMiddleware(innerScope);

			app.UseHsts(_configuration.HstsHeaderMaxAge, _configuration.HstsIncludeSubdomains);
			app.UseCors(CorsOptions.AllowAll);
			UseOAuthSecurity(app, _configuration, _authorizationServerProvider);
			MapSignalR(app, innerScope, _configuration);
			UseWebApi(app, httpConfig, innerScope);
			UseFileServer(app, _configuration, _logger);
		}

		private static void InitializeAndMigrateDatabase()
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<RelayContext, Migrations.Configuration>());

			var migrator  = new DbMigrator(new Migrations.Configuration
			{
				AutomaticMigrationsEnabled = true,
				AutomaticMigrationDataLossAllowed = false
			});

			if (migrator.GetPendingMigrations().Any())
			{
				migrator.Update();
			}
		}

		private static ILifetimeScope RegisterAdditionalServices(ILifetimeScope container, HttpConfiguration httpConfig, IConfiguration config)
		{
			return container.BeginLifetimeScope(builder =>
			{
				// This enables property injection into ASP.NET MVC filter attributes
				builder.RegisterWebApiFilterProvider(httpConfig);
				RegisterApiControllers(builder, config);
			});
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

		private static void UseOAuthSecurity(IAppBuilder app, IConfiguration config, IOAuthAuthorizationServerProvider authProvider)
		{
			if (app == null)
				throw new ArgumentNullException(nameof(app));
			if (authProvider == null)
				throw new ArgumentNullException(nameof(authProvider));

			var serverOptions = new OAuthAuthorizationServerOptions
			{
				AllowInsecureHttp = config.UseInsecureHttp,
				TokenEndpointPath = new PathString("/token"),
				AccessTokenExpireTimeSpan = TimeSpan.FromDays(365),
				Provider = authProvider,
			};

			var sharedSecret = config.OAuthSharedSecret;

			if (!String.IsNullOrWhiteSpace(sharedSecret))
			{
				UseSharedSecret(app, sharedSecret, serverOptions);
				return;
			}

			var certBase64 = config.OAuthCertificate;
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
			const string issuer = "http://thinktecture.com/relayserver/sts";
			const string audience = "http://thinktecture.com/relayserver/consumers";
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

			const string path = "/signalr";
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
				IncludeErrorDetailPolicy = configuration.IncludeErrorDetailPolicy,
			};

			httpConfig.EnableCors(new EnableCorsAttribute("*", "*", "*"));

			httpConfig.SuppressDefaultHostAuthentication();

			if (StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableTraceWriter"], "true"))
				httpConfig.Services.Replace(typeof(System.Web.Http.Tracing.ITraceWriter), new TraceWriter(logger?.ForContext<TraceWriter>(), new TraceLevelConverter()));

			httpConfig.Services.Add(typeof(IExceptionLogger), new ExceptionLogger(logger?.ForContext<ExceptionLogger>()));

			if (StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableActionFilter"], "true"))
				httpConfig.Filters.Add(new LogActionFilter(logger?.ForContext<LogActionFilter>()));

			httpConfig.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));

			if (configuration.EnableRelaying != ModuleBinding.False)
			{
				logger?.Information("Relaying enabled");
				httpConfig.Routes.MapHttpRoute("ClientRequest", "relay/{*path}", new { controller = "Client", action = "Relay" });
			}

			if (configuration.EnableOnPremiseConnections != ModuleBinding.False)
			{
				logger?.Information("On-premise connections enabled");
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetResponse", "forward", new { controller = "Response", action = "Forward" });
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetRequest", "request/{requestId}", new { controller = "Request", action = "Get" });
			}

			if (configuration.EnableManagementWeb != ModuleBinding.False)
			{
				logger?.Information("Management web enabled");
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
				const string path = "/managementweb";

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

				options.StaticFileOptions.OnPrepareResponse = (ctx) =>
				{
					ctx.OwinContext.Response.Headers.Append("X-Frame-Options", "DENY");
					ctx.OwinContext.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
				};

				app.UseFileServer(options);
			}
			catch (DirectoryNotFoundException)
			{
				// no admin web deployed - catch silently, but display info for the user
				logger?.Information("The configured directory for the ManagementWeb was not found. ManagementWeb will be disabled.");
			}
		}
	}
}
