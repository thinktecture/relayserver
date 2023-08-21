using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Maintenance;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql;

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
				var authorityUri = new Uri(Configuration.GetValue<string>("Authentication:Authority"));
				options.Authority = authorityUri.AbsoluteUri;
				options.Audience = Constants.AuthenticationAudience;
				options.RequireHttpsMetadata = authorityUri.Scheme == "https";
			});

		services.AddRelayServerDbContext(Configuration.GetConnectionString("PostgreSql")
			?? throw new InvalidOperationException("No 'PostgreSql' connection string found."));
		services.AddRelayServer(options => Configuration.GetSection("RelayServer").Bind(options))
			.AddRabbitMqRouting(options => Configuration.GetSection("RabbitMq").Bind(options))
			.AddSignalRConnectorTransport()
			.AddFileBodyStore(options => Configuration.GetSection("BodyStore").Bind(options))
			.AddMaintenanceJobs(options => Configuration.GetSection("Maintenance").Bind(options));

		services.Configure<StatisticsOptions>(options => Configuration.GetSection("Statistics").Bind(options));
	}

	// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
	{
		if (env.IsDevelopment())
		{
			app.UseDeveloperExceptionPage();
		}
		else
		{
			app.UseHttpsRedirection();
		}

		app.UseRouting();

		app.UseAuthentication();
		app.UseAuthorization();

		app.UseEndpoints(endpoints =>
		{
			endpoints.MapControllers();
		});

		app.UseRelayServer();
	}
}
