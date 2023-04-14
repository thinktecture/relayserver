using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Thinktecture.Relay.Docker;
using Thinktecture.Relay.Docker.Authentication;
using Thinktecture.Relay.Server.Management;
using Thinktecture.Relay.Server.Management.Extensions;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore.PostgreSql;

try
{
	const string applicationName = "ManagementApi";

	var builder = WebApplication.CreateBuilder(args);

	builder.Logging
		.ClearProviders()
		.AddSerilog(new LoggerConfiguration()
			.MinimumLevel.Information()
			.Destructure.With<StreamDestructuringPolicy>()
			.Enrich.FromLogContext()
			.Enrich.WithProperty("Application", applicationName)
			.ReadFrom.Configuration(builder.Configuration)
			.WriteTo.Console()
			.CreateBootstrapLogger()
		);

	// Register the db context, which in turn also registers the persistence services
	builder.Services.AddRelayServerDbContext(builder.Configuration.GetConnectionString("PostgreSql")
		?? throw new InvalidOperationException("No 'PostgreSql' connection string found."));

	// Example: Add some authentication system, in this example we use api keys defined in the config file
	builder.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
		.AddApiKey(ApiKeyAuthenticationDefaults.AuthenticationScheme, "Api Key", o =>
			builder.Configuration.GetSection("Authentication:ApiKey").Bind(o));

	// Example: Add policies and corresponding requirements for our chosen authentication
	// Here we use the default policy names
	builder.Services.AddAuthorization(o =>
	{
		o.AddPolicy(ManagementApiPolicyNames.Read, c =>
			c.RequireClaim("managementapi", new[] { "read", "readwrite", }));

		o.AddPolicy(ManagementApiPolicyNames.Write, c =>
			c.RequireClaim("managementapi", new[] { "write", "readwrite", }));
	});

	// Example: Add health checks support
	builder.Services.AddHealthChecks()
		.AddCheck("Ready check", () => HealthCheckResult.Healthy(), new[] { "Ready", })
		.AddDbContextCheck<RelayDbContext>("Db Context", tags: new[] { "Healthy", });

	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen(options =>
	{
		options.SwaggerDoc(applicationName, new OpenApiInfo()
		{
			Version = "v1",
			Title = "RelayServer Example Management API",
			Description = "An example API to manage RelayServer configuration",
		});

		options.EnableAnnotations();
	});

	var app = builder.Build();

	// Add auth to request pipeline
	app.UseAuthentication();
	app.UseAuthorization();

	// This adds the default management api endpoints at the default paths ( /api/management/... )
	// with the default authorization policies
	app.UseRelayServerManagementEndpoints();

	// Add health check endpoints for readyness and healthyness
	app.UseHealthChecks("/ready", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("Ready") });
	app.UseHealthChecks("/healthy", new HealthCheckOptions() { Predicate = check => check.Tags.Contains("Healthy") });

	app.UseSwagger(options => options.RouteTemplate = "/docs/{DocumentName}/openapi.json");
	app.UseSwaggerUI(options =>
	{
		options.RoutePrefix = String.Empty;
		options.SwaggerEndpoint($"/docs/{applicationName}/openapi.json", "RelayServer Example Management API");
	});

	await app.RunAsync();
}
catch (Exception ex)
{
	Console.WriteLine("A fatal error caused the service to crash: {0}", ex);
	Log.Fatal(ex, "A fatal error caused the service to crash");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}

return 0;
