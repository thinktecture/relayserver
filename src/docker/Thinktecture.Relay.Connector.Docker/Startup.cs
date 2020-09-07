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
				.AddWebTarget("swapi", new RelayWebTargetOptions(new Uri("https://swapi.dev/")));

			services.AddHostedService<ConnectorService>();
		}
	}

	internal class ConnectorService : IHostedService, IDisposable
	{
		private readonly ILogger<ConnectorService> _logger;
		private readonly RelayConnector _connector;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		private Task _runningTask;

		public ConnectorService(ILogger<ConnectorService> logger, RelayConnector connector)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connector = connector ?? throw new ArgumentNullException(nameof(connector));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_runningTask = Task.WhenAny(
				RunAsync(_cancellationTokenSource.Token),
				_connector.ConnectAsync(cancellationToken)
			);

			// If the task is completed then return it,
			// this will bubble cancellation and failure to the caller
			if (_runningTask.IsCompleted)
			{
				return _runningTask;
			}

			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			if (_runningTask == null)
			{
				return;
			}

			try
			{
				// Try to stop the running task.
				_cancellationTokenSource.Cancel();
			}
			finally
			{
				// Wait for the task to stop
				await Task.WhenAny(_runningTask, _connector.DisconnectAsync(cancellationToken), Task.Delay(Timeout.Infinite, cancellationToken));
			}
		}

		private async Task RunAsync(CancellationToken cancellationToken)
		{
			var i = 0;

			while (!cancellationToken.IsCancellationRequested)
			{
				_logger.LogInformation("Internal loop running at {Time} and counting {i}", DateTime.UtcNow, i++);
				await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
			}
		}

		public void Dispose() => _cancellationTokenSource.Cancel();
	}
}
