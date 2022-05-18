using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Thinktecture.Relay.Server.HealthChecks;

/// <inheritdoc/>
// ReSharper disable once ClassNeverInstantiated.Global
public class TransportHealthCheck : IHealthCheck
{
	/// <inheritdoc/>
	public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context,
		CancellationToken cancellationToken = default)
		=> Task.FromResult(HealthCheckResult.Healthy());
}
