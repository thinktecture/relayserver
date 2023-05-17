
using System.Security.Cryptography;
using System.Text;
using ExampleRelayServer.Samples;
using IdentityServer4.Models;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Thinktecture.Relay.IdentityServer.Stores;
using Thinktecture.Relay.Server;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.Maintenance;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Transport;

var builder = WebApplication.CreateBuilder(args);

builder.Host
	.UseSerilog((hostContext, _, loggerConfiguration) =>
	{
		loggerConfiguration
			.MinimumLevel.Information()
			.WriteTo.Console()
			.ReadFrom.Configuration(hostContext.Configuration);
	});


// Enable Auto-TLS
if (!String.IsNullOrEmpty(builder.Configuration.GetValue<string>("LettuceEncrypt:DomainNames:0")))
{
	Log.Information("Enabling Auto-HTTPS via LettuceEncrypt...");
	builder.Services.AddLettuceEncrypt(options =>
		builder.Configuration.GetSection("LettuceEncrypt").Bind(options));
}

// Accept forwarded headers for IdSrv
builder.Services
	.Configure<ForwardedHeadersOptions>(options =>
	{
		options.ForwardedHeaders = ForwardedHeaders.All;
		options.AllowedHosts.Add("*");
		options.KnownNetworks.Clear();
		options.KnownProxies.Clear();
	});

// Db Context
builder.Services
	.AddDbContext<RelayDbContext>(optionsBuilder =>
	{
		optionsBuilder.UseInMemoryDatabase("RelayServer");
	})
	.AddRelayServerEntityFrameworkCoreRepositories();

// Identity Server
builder.Services
	.AddIdentityServer(options => options.InputLengthRestrictions.ClientSecret = 200)
	.AddClientStore<RelayServerTenantStore>()
	.AddInMemoryApiScopes(new[] { new ApiScope("relaying") })
	.AddInMemoryApiResources(new[] { new ApiResource("relayserver") { Scopes = new[] { "relaying" } } })
	.AddRotatingFileStore(builder.Configuration.GetSection("CertificateStore"));

// AuthN/Z
builder.Services
	.AddAuthentication(Constants.DefaultAuthenticationScheme)
	.AddJwtBearer(Constants.DefaultAuthenticationScheme, options =>
	{
		var authorityUri = new Uri(builder.Configuration.GetValue<string>("Authentication:Authority"));
		options.Authority = authorityUri.AbsoluteUri;
		options.Audience = Constants.AuthenticationAudience;
		options.RequireHttpsMetadata = authorityUri.Scheme == "https";
	});

// RelayServer
builder.Services.AddControllers();
builder.Services
	.AddRelayServer(options => builder.Configuration.GetSection("RelayServer").Bind(options))
	.AddRabbitMqRouting(options => builder.Configuration.GetSection("RabbitMq").Bind(options))
	.AddSignalRConnectorTransport()
	.AddFileBodyStore(options => builder.Configuration.GetSection("BodyStore").Bind(options))
	.AddMaintenanceJobs(options => builder.Configuration.GetSection("Maintenance").Bind(options));

builder.Services
	.Configure<StatisticsOptions>(options => builder.Configuration.GetSection("Statistics").Bind(options));

// Custom Samples, optionally override default services from RelayServer
builder.Services
	.AddScoped<IRelayRequestLogger<ClientRequest, TargetResponse>, SampleMetadataRequestLogger<ClientRequest, TargetResponse>>();


var app = builder.Build();

try
{
	// Prepare Database
	using (var scope = app.Services.CreateScope())
	{
		var tenantService = scope.ServiceProvider.GetRequiredService<ITenantService>();

		foreach (var tenantConfig in app.Configuration.GetSection("Tenants").GetChildren())
		{
			var tenant = new Tenant()
			{
				Id = tenantConfig.GetValue<Guid>("id"),
				Name = tenantConfig.GetValue<string>("name"),
				ClientSecrets = new List<ClientSecret>()
				{
					new ClientSecret()
					{
						Created = DateTime.UtcNow,
						Value = Sha512(tenantConfig.GetValue<string>("clientSecret")),
					},
				},
			};

			await tenantService.CreateTenantAsync(tenant);
		}
	}

	app.UseForwardedHeaders();
	app.UseRouting();

	// Only using client credentials flow here, so no need to have this after Auth
	app.UseIdentityServer();

	app.UseAuthentication();
	app.UseAuthorization();

	app.MapControllers();
	app.UseRelayServer();

	// Let's give the Azure platform some time to route here
	Log.Information("Waiting 30 seconds before starting RelayServer...");
	await Task.Delay(TimeSpan.FromSeconds(30));

	await app.RunAsync();
	return 0;
}
catch (Exception ex)
{
	Console.WriteLine("A fatal error caused service crash: {0}", ex);
	Log.Fatal(ex, "A fatal error caused service crash");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}

static string? Sha512(string? input)
{
	if (String.IsNullOrEmpty(input)) return null;

	using var sha = SHA512.Create();
	var bytes = Encoding.UTF8.GetBytes(input);
	var hash = sha.ComputeHash(bytes);

	return Convert.ToBase64String(hash);
}
