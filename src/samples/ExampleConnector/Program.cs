using ExampleConnector;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

var host = Host.CreateDefaultBuilder();

host.UseSerilog((context, configuration) =>
{
	configuration
		.ReadFrom.Configuration(context.Configuration)
		.WriteTo.Console();
});

host
	.UseConsoleLifetime()
	.UseSystemd()
	.UseWindowsService();

host.ConfigureServices((context, services) =>
{
	services
		.AddRelayConnector(options => context.Configuration.GetSection("RelayConnector").Bind(options))
		.AddSignalRConnectorTransport();

	services.AddHostedService<ConnectorService>();
});

var app = host.Build();

try
{
	await app.RunAsync();
	return 0;
}
catch (Exception ex)
{
	Log.Fatal(ex, "Host terminated unexpectedly");
	return 1;
}
finally
{
	Log.CloseAndFlush();
}
