using System;
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
		/// <returns></returns>
		Task SetStartupTimeAsync(Guid originId);

		/// <summary>
		/// Updates the last seen time stamp of an origin statistics entry.
		/// </summary>
		/// <param name="originId">The id of the origin to update.</param>
		/// <returns></returns>
		Task UpdateLastSeenTimeAsync(Guid originId);

		/// <summary>
		/// Updates the statistic entry of an origin when it shuts down.
		/// </summary>
		/// <param name="originId">The id of the origin to mark as stopped.</param>
		/// <returns></returns>
		Task SetShutdownTimeAsync(Guid originId);
	}
}
