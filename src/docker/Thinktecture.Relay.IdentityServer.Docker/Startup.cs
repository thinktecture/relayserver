using IdentityServer4.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.IdentityServer.Stores;

namespace Thinktecture.Relay.IdentityServer.Docker;

public class Startup
{
	private IConfiguration Configuration { get; }

	public Startup(IConfiguration configuration)
		=> Configuration = configuration;

	// This method gets called by the runtime. Use this method to add services to the container.
	public void ConfigureServices(IServiceCollection services)
	{
		services.AddControllersWithViews();

		services.AddRelayServerDbContext(Configuration);

		services.AddIdentityServer(options => options.InputLengthRestrictions.ClientSecret = 200)
			.AddClientStore<RelayServerTenantStore>()
			.AddInMemoryApiScopes(new[] { new ApiScope("connector") })
			.AddInMemoryApiResources(new[] { new ApiResource("relayserver") { Scopes = new[] { "connector" } } })
			.AddRotatingFileStore(Configuration.GetSection("CertificateStore"));
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
		}

		app.UseForwardedHeaders();
		app.UseStaticFiles();

		app.UseRouting();

		app.UseAuthorization();
		app.UseAuthentication();

		app.UseIdentityServer();

		app.UseEndpoints(endpoints => endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}"));
	}
}
