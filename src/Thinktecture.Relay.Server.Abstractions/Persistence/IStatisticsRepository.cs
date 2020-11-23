using System;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Repository that allows access to statistical data.
	/// </summary>
	public interface IStatisticsRepository
	{
		/// <summary>
		/// Creates a new entry for a origin.
		/// </summary>
		/// <param name="originId">The id of the origin to store.</param>
		/// <returns></returns>
		Task CreateOriginAsync(Guid originId);

		/// <summary>
		/// Updates the heartbeat of an origin.
		/// </summary>
		/// <param name="originId">The id of the origin to update.</param>
		/// <returns></returns>
		Task HeartbeatOriginAsync(Guid originId);

		/// <summary>
		/// Updates the shutdown info of an origin.
		/// </summary>
		/// <param name="originId">The id of the origin to mark as stopped.</param>
		/// <returns></returns>
		Task ShutdownOriginAsync(Guid originId);

		/// <summary>
		/// Cleans up stale origins older than the specified timespan.
		/// </summary>
		/// <param name="oldestToKeep">The time span in which to still keep old entries.</param>
		/// <returns></returns>
		Task CleanUpOriginsAsync(TimeSpan oldestToKeep);
	}
}
