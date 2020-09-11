using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql;
using Thinktecture.Relay.Server.Protocols.RabbitMq;

namespace Thinktecture.Relay.Server.Docker
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

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

			services.AddRelayServerConfigurationDbContext(Configuration.GetConnectionString("PostgreSql"));
			services.AddRelayServer()
				.AddRabbitMqRouting(options => Configuration.GetSection("RabbitMq").Bind(options))
				.AddSignalRConnectorTransport()
				.AddFileBodyStore(options => Configuration.GetSection("BodyStore").Bind(options));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}

			// app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
			});

			app.UseRelayServer();
		}
	}
}
