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

			services.AddRelayServerDbContext(Configuration.GetConnectionString("PostgreSql"));

			services.AddIdentityServer(options => options.InputLengthRestrictions.ClientSecret = 200)
				.AddClientStore<RelayServerTenantStore>()
				.AddDeveloperSigningCredential()
				.AddInMemoryApiScopes(new[] { new ApiScope("relaying") })
				.AddInMemoryApiResources(new[] { new ApiResource("relayserver") { Scopes = new[] { "relaying" } } });
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
				app.UseHttpsRedirection();
			}

			app.UseStaticFiles();

			app.UseIdentityServer();

			app.UseRouting();

			app.UseAuthorization();
			app.UseAuthentication();

			app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"));
		}
	}
}
