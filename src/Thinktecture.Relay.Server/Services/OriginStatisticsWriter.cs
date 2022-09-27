using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Persistence;

namespace Thinktecture.Relay.Server.Services;

/// <inheritdoc/>
public class OriginStatisticsWriter : IOriginStatisticsWriter
{
	private readonly IServiceProvider _serviceProvider;

	/// <summary>
	/// Initializes a new instance of the <see cref="OriginStatisticsWriter"/> class.
	/// </summary>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
	public OriginStatisticsWriter(IServiceProvider serviceProvider)
		=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

	/// <inheritdoc/>
	public async Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.SetStartupTimeAsync(originId, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.UpdateLastSeenTimeAsync(originId, cancellationToken);
	}

	/// <inheritdoc/>
	public async Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default)
	{
		using var scope = _serviceProvider.CreateScope();
		await scope.ServiceProvider.GetRequiredService<IStatisticsService>()
			.SetShutdownTimeAsync(originId, cancellationToken);
	}
}
