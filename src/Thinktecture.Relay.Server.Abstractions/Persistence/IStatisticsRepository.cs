using System;
using System.Net;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Repository that allows access to statistical data.
	/// </summary>
	public interface IStatisticsRepository
	{
		/// <summary>
		/// Writes a new statistics entry when an origin started.
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

		/// <summary>
		/// Cleans up stale origins older than the specified timespan.
		/// </summary>
		/// <param name="maxAge">The time span in which to still keep old entries.</param>
		/// <returns></returns>
		Task CleanUpOriginsAsync(TimeSpan maxAge);

		/// <summary>
		/// Creates a new statistics entry for a connection.
		/// </summary>
		/// <param name="connectionId">The connection id from the corresponding transport.</param>
		/// <param name="tenantId">The id of the tenant this connection is created for.</param>
		/// <param name="originId">The id of the server this connection is created to.</param>
		/// <param name="remoteIpAddress">The remote ip address that initiated this connection.</param>
		/// <returns></returns>
		Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress);

		/// <summary>
		/// Updates the last activity of a connection.
		/// </summary>
		/// <param name="connectionId">The id of the connection that showed an activity.</param>
		/// <returns></returns>
		Task UpdateLastActivityTimeAsync(string connectionId);

		/// <summary>
		/// Writes the information that a connection was shut down.
		/// </summary>
		/// <param name="connectionId">The id of the connection to mark as stopped.</param>
		/// <returns></returns>
		Task SetDisconnectTimeAsync(string connectionId);

		/// <summary>
		/// Cleans up stale connections older than the specified timespan.
		/// </summary>
		/// <param name="maxAge">The time span in which to still keep old entries.</param>
		/// <returns></returns>
		Task CleanUpConnectionsAsync(TimeSpan maxAge);
	}
}
