using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
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
using Owin.Security.AesDataProtectorProvider;
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
		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly ILifetimeScope _rootScope;
		private readonly IOAuthAuthorizationServerProvider _authorizationServerProvider;

		public Startup(ILogger logger, IConfiguration configuration, ILifetimeScope rootScope, IOAuthAuthorizationServerProvider authorizationServerProvider)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_rootScope = rootScope ?? throw new ArgumentNullException(nameof(rootScope));
			_authorizationServerProvider = authorizationServerProvider ?? throw new ArgumentNullException(nameof(authorizationServerProvider));
		}

		public void Configuration(IAppBuilder app)
		{
			InitializeAndMigrateDatabase();

			var httpConfig = CreateHttpConfiguration();
			var innerScope = RegisterAdditionalServices(_rootScope, httpConfig, _configuration);

			app.UseAutofacMiddleware(innerScope);

			app.UseHsts(_configuration.HstsHeaderMaxAge, _configuration.HstsIncludeSubdomains);
			app.UseCors(CorsOptions.AllowAll);

			UseOAuthSecurity(app);
			MapSignalR(app, innerScope);
			UseWebApi(app, httpConfig, innerScope);
			UseFileServer(app);

			httpConfig.EnsureInitialized();
		}

		private static void InitializeAndMigrateDatabase()
		{
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<RelayContext, Migrations.Configuration>());

			var migrator = new DbMigrator(new Migrations.Configuration
			{
				AutomaticMigrationsEnabled = true,
				AutomaticMigrationDataLossAllowed = false
			});

			if (migrator.GetPendingMigrations().Any())
			{
				migrator.Update();
			}
		}

		private ILifetimeScope RegisterAdditionalServices(ILifetimeScope container, HttpConfiguration httpConfig, IConfiguration config)
		{
			return container.BeginLifetimeScope(builder =>
			{
				// This enables property injection into ASP.NET MVC filter attributes
				builder.RegisterWebApiFilterProvider(httpConfig);
				RegisterApiControllers(builder);

				// Make the current <see cref="HttpRequestMessage"/> resolvable through the dependency scope.
				// This is required to extract the current HttpContext from its properties.
				builder.RegisterHttpRequestMessage(httpConfig);
				builder.Register(c => c.Resolve<HttpRequestMessage>().Properties["MS_RequestContext"])
					.As<HttpRequestContext>()
					.InstancePerRequest();
			});
		}

		private void RegisterApiControllers(ContainerBuilder builder)
		{
			if (_configuration.EnableManagementWeb != ModuleBinding.False)
				builder.RegisterModule<ManagementWebModule>();

			if (_configuration.EnableRelaying != ModuleBinding.False)
				builder.RegisterModule<RelayingModule>();

			if (_configuration.EnableOnPremiseConnections != ModuleBinding.False)
				builder.RegisterModule<OnPremiseConnectionsModule>();
		}

		private void UseOAuthSecurity(IAppBuilder app)
		{
			// Todo: Add this only when relaying is enabled (no need to auth OnPremises if not), and add
			// a second endpoint with different token lifetime for management web user (i.e. `/managementToken`),
			// when management web is enabled. Also, use different AuthProviders for each endpoint
			var serverOptions = new OAuthAuthorizationServerOptions
			{
				AllowInsecureHttp = _configuration.UseInsecureHttp,
				TokenEndpointPath = new PathString("/token"),
				AccessTokenExpireTimeSpan = _configuration.AccessTokenLifetime,
				Provider = _authorizationServerProvider,
			};

			var certBase64 = _configuration.OAuthCertificate;

			if (!String.IsNullOrWhiteSpace(certBase64))
			{
				var cert = new X509Certificate2(Convert.FromBase64String(certBase64));

				serverOptions.AccessTokenFormat = new TicketDataFormat(new RsaDataProtector(cert));

				var authOptions = new OAuthBearerAuthenticationOptions
				{
					AccessTokenFormat = new TicketDataFormat(new RsaDataProtector(cert)),
					Provider = new OAuthBearerAuthenticationProvider()
					{
						OnApplyChallenge = context =>
						{
							// Workaround: Keep an already set WWW-Authenticate header (otherwise OWIN would add its challenge). 
							if (!context.Response.Headers.ContainsKey("WWW-Authenticate"))
							{
								context.OwinContext.Response.Headers.AppendValues("WWW-Authenticate", context.Challenge);
							}

							return Task.CompletedTask;
						}
					}
				};

				app.UseOAuthBearerAuthentication(authOptions);
			}
			else
			{
				var sharedSecret = _configuration.SharedSecret;
				if (!String.IsNullOrWhiteSpace(sharedSecret))
				{
					const string issuer = "http://thinktecture.com/relayserver/sts";
					const string audience = "http://thinktecture.com/relayserver/consumers";

					var key = Convert.FromBase64String(sharedSecret);

					serverOptions.AccessTokenFormat = new CustomJwtFormat(serverOptions.AccessTokenExpireTimeSpan, key, issuer, audience);

					app.UseJwtBearerAuthentication(new JwtBearerAuthenticationOptions()
					{
						AllowedAudiences = new[] { audience },
						IssuerSecurityTokenProviders = new[] { new SymmetricKeyIssuerSecurityTokenProvider(issuer, key) },
					});
				}
			}

			app.UseOAuthAuthorizationServer(serverOptions);
		}

		private void MapSignalR(IAppBuilder app, ILifetimeScope scope)
		{
			if (_configuration.EnableOnPremiseConnections == ModuleBinding.False)
				return;

			const string path = "/signalr";
			GlobalHost.Configuration.ConnectionTimeout = TimeSpan.FromSeconds(_configuration.ConnectionTimeout);
			GlobalHost.Configuration.DisconnectTimeout = TimeSpan.FromSeconds(_configuration.DisconnectTimeout);
			GlobalHost.Configuration.KeepAlive = TimeSpan.FromSeconds(_configuration.KeepAliveInterval);

			var sharedSecret = _configuration.SharedSecret;
			if (!String.IsNullOrWhiteSpace(sharedSecret))
			{
				app.UseAesDataProtectorProvider(sharedSecret);
			}

			if (_configuration.EnableOnPremiseConnections == ModuleBinding.Local)
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

		private HttpConfiguration CreateHttpConfiguration()
		{
			var httpConfig = new HttpConfiguration
			{
				IncludeErrorDetailPolicy = _configuration.IncludeErrorDetailPolicy,
			};

			httpConfig.EnableCors(new EnableCorsAttribute("*", "*", "*"));

			httpConfig.SuppressDefaultHostAuthentication();

			if (StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableTraceWriter"], "true"))
				httpConfig.Services.Replace(typeof(System.Web.Http.Tracing.ITraceWriter), new TraceWriter(_logger?.ForContext<TraceWriter>(), new TraceLevelConverter()));

			httpConfig.Services.Add(typeof(IExceptionLogger), new ExceptionLogger(_logger?.ForContext<ExceptionLogger>()));

			if (StringComparer.OrdinalIgnoreCase.Equals(ConfigurationManager.AppSettings["EnableActionFilter"], "true"))
				httpConfig.Filters.Add(new LogActionFilter(_logger?.ForContext<LogActionFilter>()));

			httpConfig.Filters.Add(new HostAuthenticationFilter(OAuthDefaults.AuthenticationType));
			// custom code api controllers should use the RouteAttribute only

			httpConfig.MapHttpAttributeRoutes();

			if (_configuration.EnableRelaying != ModuleBinding.False)
			{
				_logger?.Information("Relaying enabled");
				httpConfig.Routes.MapHttpRoute("ClientRequest", "relay/{*fullPathToOnPremiseEndpoint}", new { controller = "Client", action = "Relay" });
			}

			if (_configuration.EnableOnPremiseConnections != ModuleBinding.False)
			{
				_logger?.Information("On-premise connections enabled");
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetResponse", "forward", new { controller = "Response", action = "Forward" });
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetRequestAcknowledgement", "request/acknowledge", new { controller = "Request", action = "Acknowledge" });
				httpConfig.Routes.MapHttpRoute("OnPremiseTargetRequest", "request/{requestId}", new { controller = "Request", action = "Get" });
			}

			if (_configuration.EnableManagementWeb != ModuleBinding.False)
			{
				_logger?.Information("Management web enabled");
				httpConfig.Routes.MapHttpRoute("ManagementWeb", "api/managementweb/{controller}/{action}");
			}

			httpConfig.Formatters.JsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

			return httpConfig;
		}

		private void UseFileServer(IAppBuilder app)
		{
			if (_configuration.EnableManagementWeb == ModuleBinding.False)
				return;

			try
			{
				const string path = "/managementweb";

				var options = new FileServerOptions()
				{
					FileSystem = new PhysicalFileSystem(_configuration.ManagementWebLocation),
					RequestPath = new PathString(path),
				};
				options.DefaultFilesOptions.DefaultFileNames.Add("index.html");

				if (_configuration.EnableManagementWeb == ModuleBinding.Local)
				{
					app.Use(typeof(BlockNonLocalRequestsMiddleware), path);
				}

				options.StaticFileOptions.OnPrepareResponse = ctx =>
				{
					ctx.OwinContext.Response.Headers.Append("X-Frame-Options", "DENY");
					ctx.OwinContext.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
				};

				app.UseFileServer(options);
			}
			catch (DirectoryNotFoundException)
			{
				// no admin web deployed - catch silently, but display info for the user
				_logger?.Information("The configured directory for the ManagementWeb was not found. ManagementWeb will be disabled.");
			}
		}
	}
}
