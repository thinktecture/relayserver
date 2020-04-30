using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.IdentityServer.Stores;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql;

namespace Thinktecture.Relay.IdentityServer.Docker
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
			services.AddControllersWithViews();

			services.AddPostgreSqlRelayServerConfigurationDbContext(Configuration.GetConnectionString("PostgreSql"));

			services.AddIdentityServer(c => { })
				.AddClientStore<RelayServerTenantStore>()
				.AddDeveloperSigningCredential()
				.AddInMemoryApiResources(new[]
				{
					new ApiResource("RelayServer")
					{
						Scopes = new[]
						{
							new Scope("relaying"),
						},
					},
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
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseIdentityServer();

			app.UseRouting();

			app.UseAuthorization();
			app.UseAuthentication();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
						name: "default",
						pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
