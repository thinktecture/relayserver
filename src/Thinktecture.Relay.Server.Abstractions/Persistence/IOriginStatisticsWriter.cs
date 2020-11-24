using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Adapter that allows scoped writing to statistics for origins.
	/// </summary>
	public interface IOriginStatisticsWriter
	{
		/// <summary>
		/// Creates a new entry for an origin.
		/// </summary>
		/// <param name="originId">The id of the origin to store.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the last seen time stamp of an origin statistics entry.
		/// </summary>
		/// <param name="originId">The id of the origin to update.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the statistic entry of an origin when it shuts down.
		/// </summary>
		/// <param name="originId">The id of the origin to mark as stopped.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default);
	}
}
