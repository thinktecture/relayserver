using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Server.Maintenance;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services;

/// <summary>
/// An implementation that writes server startup, shutdown to
/// </summary>
public class ServerStatisticsWriter : BackgroundService
{
	private readonly RelayServerContext _serverContext;
	private readonly StatisticsOptions _statisticsOptions;
	private readonly IOriginStatisticsWriter _originWriter;
	private readonly IConnectionStatisticsWriter _connectionWriter;

	/// <summary>
	/// Initializes a new instance of the <see cref="OriginStatisticsWriter"/> class.
	/// </summary>
	/// <param name="originWriter">An instance of an <see cref="IOriginStatisticsWriter"/>.</param>
	/// <param name="connectionWriter">An instance of an <see cref="IConnectionStatisticsWriter"/>.</param>
	/// <param name="serverContext">An instance of an <see cref="RelayServerContext"/>.</param>
	/// <param name="statisticsOptions">An instance of an <see cref="IOptions{StatisticsOptions}"/>.</param>
	public ServerStatisticsWriter(
		IOriginStatisticsWriter originWriter,
		IConnectionStatisticsWriter connectionWriter,
		RelayServerContext serverContext,
		IOptions<StatisticsOptions> statisticsOptions)
	{
		if (statisticsOptions == null) throw new ArgumentNullException(nameof(statisticsOptions));

		_originWriter = originWriter ?? throw new ArgumentNullException(nameof(originWriter));
		_connectionWriter = connectionWriter ?? throw new ArgumentNullException(nameof(connectionWriter));
		_serverContext = serverContext ?? throw new ArgumentNullException(nameof(serverContext));
		_statisticsOptions = statisticsOptions.Value;
	}

	/// <inheritdoc />
	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		await _originWriter.SetStartupTimeAsync(_serverContext.OriginId, cancellationToken);
		await base.StartAsync(cancellationToken);
	}

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			// launch and await multiple writing loops with different writing intervals
			await Task.WhenAll(
				ExecuteOriginUpdatesAsync(stoppingToken),
				ExecuteConnectionUpdatesAsync(stoppingToken)
			);
		}
		catch (OperationCanceledException)
		{
			// Ignore this, as this will be thrown when the service shuts down gracefully
		}
	}

	/// <inheritdoc />
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		await _originWriter.SetShutdownTimeAsync(_serverContext.OriginId, cancellationToken);
		await base.StopAsync(cancellationToken);
	}

	private async Task ExecuteOriginUpdatesAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await _originWriter.UpdateLastSeenTimeAsync(_serverContext.OriginId, stoppingToken);
			await Task.Delay(_statisticsOptions.OriginLastSeenUpdateInterval, stoppingToken);
		}
	}

	private async Task ExecuteConnectionUpdatesAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await _connectionWriter.WriteLastSeenEntriesAsync(stoppingToken);
			await Task.Delay(_statisticsOptions.ConnectionLastSeenUpdateInterval, stoppingToken);
		}
	}
}
