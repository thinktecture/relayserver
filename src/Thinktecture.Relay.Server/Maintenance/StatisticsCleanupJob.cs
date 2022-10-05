using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Maintenance;

/// <inheritdoc/>
public class StatisticsCleanupJob : IMaintenanceJob
{
	private readonly StatisticsOptions _statisticsOptions;
	private readonly IStatisticsService _statisticsService;

	/// <summary>
	/// Initializes a new instance of the <see cref="StatisticsCleanupJob"/> class.
	/// </summary>
	/// <param name="statisticsService">An <see cref="IStatisticsService"/>.</param>
	/// <param name="statisticsOptions">An <see cref="IOptions{StatisticsOptions}"/>.</param>
	public StatisticsCleanupJob(IStatisticsService statisticsService,
		IOptions<StatisticsOptions> statisticsOptions)
	{
		if (statisticsOptions == null) throw new ArgumentNullException(nameof(statisticsOptions));

		_statisticsService = statisticsService ?? throw new ArgumentNullException(nameof(statisticsService));
		_statisticsOptions = statisticsOptions.Value;
	}

	/// <inheritdoc/>
	public async Task DoMaintenanceAsync(CancellationToken cancellationToken = default)
	{
		await _statisticsService.CleanUpOriginsAsync(_statisticsOptions.EntryMaxAge, cancellationToken);

		if (_statisticsOptions.EnableConnectionCleanup)
		{
			await _statisticsService.CleanUpConnectionsAsync(_statisticsOptions.EntryMaxAge, cancellationToken);
		}
	}
}
