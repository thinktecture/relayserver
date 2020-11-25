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
				.AddSignalRConnectorTransport();

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
