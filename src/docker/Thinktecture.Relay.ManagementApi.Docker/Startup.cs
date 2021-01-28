using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql;

namespace Thinktecture.Relay.ManagementApi.Docker
{
	internal class Startup
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
			services.AddRelayServerDbContext(Configuration.GetConnectionString("PostgreSql"));

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("Management", new OpenApiInfo()
				{
					Version = "v1",
					Title = "RelayServer Management API",
					Description = "An API to manage RelayServer configuration"
				});

				options.EnableAnnotations();
				options.IncludeXmlComments("./ManagementApiDocumentation.xml");
			});
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

			app.UseEndpoints(endpoints => endpoints.MapControllers());

			app.UseSwagger(options => options.RouteTemplate = "/docs/{DocumentName}/openapi.json");
			app.UseSwaggerUI(options =>
			{
				options.RoutePrefix = string.Empty;
				options.SwaggerEndpoint("/docs/Management/openapi.json", "RelayServer Management API");
			});
		}
	}
}
