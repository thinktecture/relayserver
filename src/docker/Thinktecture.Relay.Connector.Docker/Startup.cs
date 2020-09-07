using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
				.AddWebTarget("swapi", new RelayWebTargetOptions(new Uri("http://swapi.dev/")));

			services.AddHostedService<ConnectorService>();
		}
	}

	internal class ConnectorService : IHostedService, IDisposable
	{
		private readonly RelayConnector _connector;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

		public ConnectorService(RelayConnector connector)
		{
			_connector = connector ?? throw new ArgumentNullException(nameof(connector));
		}

		public Task StartAsync(CancellationToken cancellationToken)
		{
			_connector.ConnectAsync(_cancellationTokenSource.Token);
			return Task.CompletedTask;
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_cancellationTokenSource.Cancel();
			await _connector.DisconnectAsync(cancellationToken);
		}

		public void Dispose() => _cancellationTokenSource.Cancel();
	}
}
