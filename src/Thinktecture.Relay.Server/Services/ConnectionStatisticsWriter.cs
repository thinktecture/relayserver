using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services;

/// <inheritdoc/>
public class ConnectionStatisticsWriter : IConnectionStatisticsWriter
{
	private readonly IServiceScopeFactory _serviceProvider;

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
	public async Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		// TODO debounce writing to DB; keep connectionId and UtcNow in dictionary
		// write to DB every {configurable duration} (default: 1 minute)
		// additionally add this as a background service just like server statistics writer

		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.UpdateLastActivityTimeAsync(connectionId, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.SetDisconnectTimeAsync(connectionId, cancellationToken);
	}
}
