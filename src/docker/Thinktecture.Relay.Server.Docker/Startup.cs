using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Docker.Interceptors;
using Thinktecture.Relay.Server.Endpoints;

namespace Thinktecture.Relay.Server.Docker;

public class Startup
{
	public IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
		=> Configuration = configuration;

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllers();

		services
			.AddAuthentication(Constants.DefaultAuthenticationScheme)
			.AddJwtBearer(Constants.DefaultAuthenticationScheme, options =>
			{
				var value = Configuration.GetValue<string>("Authentication:Authority") ??
					throw new ArgumentException("The configuration value 'Authentication:Authority' is missing.");
				var authorityUri = new Uri(value);
				options.Authority = authorityUri.AbsoluteUri;
				options.Audience = Constants.DefaultAuthenticationAudience;
				options.RequireHttpsMetadata = authorityUri.Scheme == "https";

				// This is a demo environment, we should try to recover fast if we can't access our Identity Provider
				// when fetching its OpenId configuration. Otherwise we would stay in failed state in the MS JwtBearer
				// ConfigurationManager until its default refresh interval of 5 minutes passed.
				options.RefreshInterval = TimeSpan.FromSeconds(15);
			});

		services.AddRelayServerDbContext(Configuration);
		services.AddRelayServer(options => Configuration.GetSection("RelayServer").Bind(options))
			.AddRabbitMqRouting(options => Configuration.GetSection("RabbitMq").Bind(options))
			.AddSignalRConnectorTransport()
			.AddFileBodyStore(options => Configuration.GetSection("BodyStore").Bind(options))
			.AddMaintenanceJobs(options => Configuration.GetSection("Maintenance").Bind(options))
			.AddForwardedHeaderInterceptor()
			.AddClientRequestInterceptor<DemoRequestInterceptor>()
			.AddTargetResponseInterceptor<DemoResponseInterceptor>();

		services.Configure<StatisticsOptions>(options => Configuration.GetSection("Statistics").Bind(options));
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseRelayServer();
	}
}
