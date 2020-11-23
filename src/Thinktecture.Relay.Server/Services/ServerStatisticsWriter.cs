using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that writes server startup, shutdown to
	/// </summary>
	public class ServerStatisticsWriter : BackgroundService
	{
		private readonly IServiceProvider _serviceProvider;
		private readonly IOriginStatisticsWriter _statisticsWriter;
		private readonly RelayServerContext _serverContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="OriginStatisticsWriter"/>.
		/// </summary>
		/// <param name="serviceProvider">The service provider to access services from.</param>
		/// <param name="statisticsWriter">An instance of an <see cref="IOriginStatisticsWriter"/>.</param>
		/// <param name="serverContext">An instance of an <see cref="RelayServerContext"/>.</param>
		public ServerStatisticsWriter(IServiceProvider serviceProvider, IOriginStatisticsWriter statisticsWriter, RelayServerContext serverContext)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
			_statisticsWriter = statisticsWriter ?? throw new ArgumentNullException(nameof(statisticsWriter));
			_serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
		}

		/// <inheritdoc />
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			await _statisticsWriter.CreateOriginAsync(_serverContext.OriginId);

			await base.StartAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await _statisticsWriter.HeartbeatOriginAsync(_serverContext.OriginId);

					using (var scope = _serviceProvider.CreateScope())
					{
						var repo = scope.ServiceProvider.GetRequiredService<IStatisticsRepository>();

						// TODO: Use configuration for timespan to keep entries for
						await repo.CleanUpOriginsAsync(TimeSpan.FromMinutes(15));
						await repo.CleanUpConnectionsAsync(TimeSpan.FromMinutes(15));
					}

					// TODO: Use configuration for cleanup
					await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
				}
			}
			catch (TaskCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
		}

		/// <inheritdoc />
		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await _statisticsWriter.ShutdownOriginAsync(_serverContext.OriginId);

			await base.StopAsync(cancellationToken);
		}
	}
}
