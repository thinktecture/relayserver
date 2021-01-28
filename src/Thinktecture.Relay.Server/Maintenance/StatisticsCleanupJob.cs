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
		/// Initializes a new instance of the <see cref="StatisticsCleanupJob"/> class.
		/// </summary>
		/// <param name="statisticsRepository">An <see cref="IStatisticsRepository"/>.</param>
		/// <param name="statisticsOptions">An <see cref="IOptions{StatisticsOptions}"/>.</param>
		public StatisticsCleanupJob(IStatisticsRepository statisticsRepository, IOptions<StatisticsOptions> statisticsOptions)
		{
			if (statisticsOptions == null) throw new ArgumentNullException(nameof(statisticsOptions));

			_statisticsRepository = statisticsRepository ?? throw new ArgumentNullException(nameof(statisticsRepository));
			_statisticsOptions = statisticsOptions.Value;
		}

		/// <inheritdoc />
		public async Task DoMaintenanceAsync(CancellationToken cancellationToken = default)
		{
			await _statisticsRepository.CleanUpOriginsAsync(_statisticsOptions.EntryMaxAge, cancellationToken);
			await _statisticsRepository.CleanUpConnectionsAsync(_statisticsOptions.EntryMaxAge, cancellationToken);
		}
	}
}
