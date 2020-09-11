using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector.RelayTargets;

namespace Thinktecture.Relay.Connector.Docker
{
	public static class Startup
	{
		// This method gets called by the runtime. Use this method to add services to the container.
		public static void ConfigureServices(HostBuilderContext hostBuilderContext, IServiceCollection services)
		{
			var configuration = hostBuilderContext.Configuration;

			services
				.AddRelayConnector(options => configuration.GetSection("RelayConnector").Bind(options))
				.AddSignalRConnectorTransport()
				// returns simple JSON ({ "Hello": "World" }) (followed by "?mocky-delay=#ms" to simulate a long running request delayed by #)
				.AddTarget<RelayWebTarget>("mocky1", TimeSpan.FromSeconds(30),
					new Uri("https://run.mocky.io/v3/ac6dd3d6-f351-4475-9bd1-c0f58030e31a"))
				// returns HTTP status NO CONTENT (followed by "?mocky-delay=#ms" to simulate a long running request delayed by #)
				.AddTarget<RelayWebTarget>("mocky2", TimeSpan.FromSeconds(2),
					new Uri("https://run.mocky.io/v3/dd0c23d8-6802-46ea-a188-675d022d0e4d"))
				// returns big JOSN (followed by "?mocky-delay=#ms" to simulate a long running request delayed by #)
				.AddTarget<RelayWebTarget>("mocky3", TimeSpan.FromSeconds(2),
					new Uri("https://run.mocky.io/v3/b0949784-114b-4ea9-80a8-f08aca93c796"))
				// returns HTTP status by appended code (followed by "?sleep=#" to simulate a long running request delayed by # msec)
				.AddTarget<RelayWebTarget>("status", TimeSpan.FromSeconds(2), new Uri("https://httpstat.us/"))
				// returns more complex JSON (e.g. "/api/people/1/")
				.AddTarget<RelayWebTarget>("swapi", default, new Uri("https://swapi.dev/"))
				// returns a random 4k image
				.AddTarget<RelayWebTarget>("picsum", default, new Uri("https://picsum.photos/3840/2160"))
				// returns a really big pdf
				.AddTarget<RelayWebTarget>("bigpdf", default,
					new Uri("https://cartographicperspectives.org/index.php/journal/article/download/cp43-complete-issue/pdf/"));

			services.AddHostedService<ConnectorService>();
		}
	}

	internal class ConnectorService : IHostedService
	{
		private readonly ILogger<ConnectorService> _logger;
		private readonly RelayConnector _connector;

		public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connector = connector ?? throw new ArgumentNullException(nameof(connector));
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting connector");
			await _connector.ConnectAsync(cancellationToken);
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Gracefully stopping connector");
			await _connector.DisconnectAsync(cancellationToken);
		}
	}
}
