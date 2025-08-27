using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Thinktecture.Relay.Docker;
using Thinktecture.Relay.Docker.Authentication;
using Thinktecture.Relay.ManagementApi.Docker;
using Thinktecture.Relay.Server.Management;
using Thinktecture.Relay.Server.Management.Extensions;
using Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

try
{
	const string applicationName = "ManagementApi";

	var builder = WebApplication.CreateBuilder(args);

	builder
		.Logging.ClearProviders()
		.AddSerilog(
			new LoggerConfiguration()
				.MinimumLevel.Information()
				.Destructure.With<StreamDestructuringPolicy>()
				.Enrich.FromLogContext()
				.Enrich.WithProperty("Application", applicationName)
				.ReadFrom.Configuration(builder.Configuration)
				.WriteTo.Console()
				.CreateBootstrapLogger()
		);

	// Register the db context, which in turn also registers the persistence services
	builder.Services.AddRelayServerDbContext(builder.Configuration);

	// Add health checks
	builder.Services.AddHealthChecks().AddDbContextCheck<RelayDbContext>("database", tags: new[] { "ready" });

	// Example: Add some authentication system, in this example we use api keys defined in the config file
	builder
		.Services.AddAuthentication(ApiKeyAuthenticationDefaults.AuthenticationScheme)
		.AddApiKey(
			ApiKeyAuthenticationDefaults.AuthenticationScheme,
			"Api Key",
			o => builder.Configuration.GetSection("Authentication:ApiKey").Bind(o)
		);

	// Example: Add policies and corresponding requirements for our chosen authentication
	// Here we use the default policy names
	builder.Services.AddAuthorization(o =>
	{
		o.AddPolicy(ManagementApiPolicyNames.Read, c => c.RequireClaim("managementapi", "read", "readwrite"));

		o.AddPolicy(ManagementApiPolicyNames.Write, c => c.RequireClaim("managementapi", "write", "readwrite"));
	});

	builder.Services.AddEndpointsApiExplorer();
	builder.Services.AddSwaggerGen(options =>
	{
		options.SwaggerDoc(
			applicationName,
			new OpenApiInfo()
			{
				Version = "v1",
				Title = "RelayServer Example Management API",
				Description = "An example API to manage RelayServer configuration",
			}
		);

		options.EnableAnnotations();
	});

	var app = builder.Build();

	// Add auth to request pipeline
	app.UseAuthentication();
	app.UseAuthorization();

	// Map health check endpoints
	app.MapHealthChecks(
		"/health/live",
		new HealthCheckOptions
		{
			Predicate = _ => false, // Liveness check - no dependencies
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = "text/plain";
				await context.Response.WriteAsync(report.Status == HealthStatus.Healthy ? "Healthy" : "Unhealthy");
			},
		}
	);

	app.MapHealthChecks(
		"/health/ready",
		new HealthCheckOptions
		{
			Predicate = check => check.Tags.Contains("ready"), // Readiness check - includes database
			ResponseWriter = async (context, report) =>
			{
				context.Response.ContentType = "text/plain";
				await context.Response.WriteAsync(report.Status == HealthStatus.Healthy ? "Ready" : "Not Ready");
			},
		}
	);

	// This adds the default management api endpoints at the default paths ( /api/management/... )
	// with the default authorization policies
	app.UseRelayServerManagementEndpoints();

	app.UseSwagger(options => options.RouteTemplate = "/docs/{DocumentName}/openapi.json");
	app.UseSwaggerUI(options =>
	{
		options.RoutePrefix = string.Empty;
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
