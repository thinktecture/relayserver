using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

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
				.AddTarget<RelayWebTarget>("mocky", new Uri("https://run.mocky.io/v3/"))
				.AddTarget<RelayWebTarget>("swapi", new Uri("https://swapi.dev/"));

			services.AddHostedService<ConnectorService>();
		}
	}

	internal class ConnectorService : IHostedService, IDisposable
	{
		private readonly ILogger<ConnectorService> _logger;
		private readonly RelayConnector _connector;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connector = connector ?? throw new ArgumentNullException(nameof(connector));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Starting connector");
			_connector.ConnectAsync(_cancellationTokenSource.Token);
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Gracefully stopping connector");
			_cancellationTokenSource.Cancel();
			await _connector.DisconnectAsync(cancellationToken);
		}

		public void Dispose() => _cancellationTokenSource.Dispose();
	}
}
