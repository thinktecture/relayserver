using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Adapter that allows writing statistics for origins.
/// </summary>
/// <remarks>
/// This class should always be registered as a singleton, because it is creating an own scope during the
/// execution of any method.
/// </remarks>
public interface IOriginStatisticsWriter
{
	/// <summary>
	/// Sets the startup time of an origin.
	/// </summary>
	/// <param name="originId">The unique id of the origin.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates the last seen time of an origin.
	/// </summary>
	/// <param name="originId">The unique id of the origin.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the shutdown time of an origin..
	/// </summary>
	/// <param name="originId">The unique id of the origin.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default);
}
