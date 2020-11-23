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
	}
}
