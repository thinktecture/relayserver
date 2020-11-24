using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Maintenance
{
	/// <inheritdoc />
	public class StatisticsCleanupJob : IMaintenanceJob
	{
		private readonly IStatisticsRepository _statisticsRepository;
		private readonly StatisticsOptions _statisticsOptions;

		/// <summary>
		/// Initializes a new instance of the <see cref="StatisticsCleanupJob"/>.
		/// </summary>
		/// <param name="statisticsRepository">An instance of an <see cref="IStatisticsRepository"/>.</param>
		/// <param name="statisticsOptions">An instance of an <see cref="IOptions{StatisticsOptions}"/>.</param>
		public StatisticsCleanupJob(IStatisticsRepository statisticsRepository, IOptions<StatisticsOptions> statisticsOptions)
		{
			_statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
			_statisticsOptions = statisticsOptions.Value ?? throw new ArgumentNullException(nameof(statisticsOptions));
		}

		/// <inheritdoc />
		public async Task DoMaintenanceAsync(CancellationToken stoppingToken)
		{
			if (stoppingToken.IsCancellationRequested)
			{
				return;
			}

			await _statisticsRepository.CleanUpOriginsAsync(_statisticsOptions.EntryMaxAge);
			await _statisticsRepository.CleanUpConnectionsAsync(_statisticsOptions.EntryMaxAge);
		}
	}
}
