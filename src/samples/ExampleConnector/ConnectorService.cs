using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector;

namespace ExampleConnector;

internal class ConnectorService : IHostedService
{
	private readonly ILogger<ConnectorService> _logger;
	private readonly RelayConnector _connector;

	public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
	{
		_logger = logger;
		_connector = connector;
	}

	public async Task StartAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Starting connector");
		await _connector.ConnectAsync(cancellationToken);
	}

	public async Task StopAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Stopping connector");
		await _connector.DisconnectAsync(cancellationToken);
	}
}
