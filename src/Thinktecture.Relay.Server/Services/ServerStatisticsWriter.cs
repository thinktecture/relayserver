using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Maintenance;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services
{
	/// <summary>
	/// An implementation that writes server startup, shutdown to
	/// </summary>
	public class ServerStatisticsWriter : BackgroundService
	{
		private readonly IOriginStatisticsWriter _statisticsWriter;
		private readonly RelayServerContext _serverContext;
		private readonly StatisticsOptions _statisticsOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="OriginStatisticsWriter"/> class.
		/// </summary>
		/// <param name="statisticsWriter">An instance of an <see cref="IOriginStatisticsWriter"/>.</param>
		/// <param name="serverContext">An instance of an <see cref="RelayServerContext"/>.</param>
		/// <param name="statisticsOptions">An instance of an <see cref="IOptions{StatisticsOptions}"/>.</param>
		public ServerStatisticsWriter(IOriginStatisticsWriter statisticsWriter, RelayServerContext serverContext,
			IOptions<StatisticsOptions> statisticsOptions)
		{
			if (statisticsOptions == null) throw new ArgumentNullException(nameof(statisticsOptions));

			_statisticsWriter = statisticsWriter ?? throw new ArgumentNullException(nameof(statisticsWriter));
			_serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
			_statisticsOptions = statisticsOptions.Value;
		}

		/// <inheritdoc />
		public override async Task StartAsync(CancellationToken cancellationToken)
		{
			await _statisticsWriter.SetStartupTimeAsync(_serverContext.OriginId, cancellationToken);
			await base.StartAsync(cancellationToken);
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				while (!stoppingToken.IsCancellationRequested)
				{
					await Task.Delay(_statisticsOptions.LastActivityUpdateInterval, stoppingToken);
					await _statisticsWriter.UpdateLastSeenTimeAsync(_serverContext.OriginId, stoppingToken);
				}
			}
			catch (OperationCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
		}

		/// <inheritdoc />
		public override async Task StopAsync(CancellationToken cancellationToken)
		{
			await _statisticsWriter.SetShutdownTimeAsync(_serverContext.OriginId, cancellationToken);
			await base.StopAsync(cancellationToken);
		}
	}
}
