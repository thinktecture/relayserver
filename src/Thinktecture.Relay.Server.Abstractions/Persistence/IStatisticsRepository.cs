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

		/// <summary>
		/// Cleans up stale origins older than the specified timespan.
		/// </summary>
		/// <param name="oldestToKeep">The time span in which to still keep old entries.</param>
		/// <returns></returns>
		Task CleanUpOriginsAsync(TimeSpan oldestToKeep);

		/// <summary>
		/// Creates a new entry for a connection.
		/// </summary>
		/// <param name="connectionId">The connection id from the corresponding transport.</param>
		/// <param name="tenantId">The id of the tenant this connection is created for.</param>
		/// <param name="originId">The id of the server this connection is created to.</param>
		/// <param name="remoteIpAddress">The remote ip address that initiated this connection.</param>
		/// <returns></returns>
		Task CreateConnectionAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress);

		/// <summary>
		/// Updates the last activity of a connection.
		/// </summary>
		/// <param name="connectionId">The id of the connection that showed an activity.</param>
		/// <returns></returns>
		Task HeartbeatConnectionAsync(string connectionId);

		/// <summary>
		/// Updates the information that a connection was shut down.
		/// </summary>
		/// <param name="connectionId">The id of the connection to mark as stopped.</param>
		/// <returns></returns>
		Task CloseConnectionAsync(string connectionId);

		/// <summary>
		/// Cleans up stale connections older than the specified timespan.
		/// </summary>
		/// <param name="oldestToKeep">The time span in which to still keep old entries.</param>
		/// <returns></returns>
		Task CleanUpConnectionsAsync(TimeSpan fromMinutes);
	}
}
