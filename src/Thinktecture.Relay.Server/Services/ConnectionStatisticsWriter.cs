using System;
using System.Collections.Concurrent;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services;

/// <inheritdoc cref="IConnectionStatisticsWriter"/>
public class ConnectionStatisticsWriter : IConnectionStatisticsWriter
{
	private readonly IServiceScopeFactory _serviceProvider;

	private ConcurrentDictionary<string, DateTimeOffset> _buffer = new ConcurrentDictionary<string, DateTimeOffset>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectionStatisticsWriter"/> class.
	/// </summary>
	/// <param name="serviceProvider">An <see cref="IServiceScopeFactory"/>.</param>
	public ConnectionStatisticsWriter(IServiceScopeFactory serviceProvider)
		=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <inheritdoc/>
	public async Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId,
		IPAddress? remoteIpAddress,
		CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.SetConnectionTimeAsync(connectionId, tenantId, originId, remoteIpAddress, cancellationToken);
	}

	/// <inheritdoc/>
	public Task UpdateLastSeenTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		_buffer[connectionId] = DateTimeOffset.UtcNow;
		return Task.CompletedTask;
	}

	/// <inheritdoc/>
	public async Task WriteLastSeenEntriesAsync(CancellationToken stoppingToken)
	{
		var buffer = _buffer;
		_buffer = new ConcurrentDictionary<string, DateTimeOffset>();

		await Task.Yield();

		using var scope = _serviceProvider.CreateScope();
		var statisticsService = scope.ServiceProvider.GetRequiredService<IStatisticsService>();

		await statisticsService.UpdateLastSeenTimeAsync(buffer, stoppingToken);
	}

	/// <inheritdoc/>
	public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.SetDisconnectTimeAsync(connectionId, cancellationToken);
	}
}
