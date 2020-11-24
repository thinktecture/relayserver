using System;
using System.Net;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Adapter that allows scoped writing to statistics for connections.
	/// </summary>
	public interface IConnectionStatisticsWriter
	{
		/// <summary>
		/// Creates a new entry for a connection.
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
		/// Updates the information that a connection was shut down.
		/// </summary>
		/// <param name="connectionId">The id of the connection to mark as stopped.</param>
		/// <returns></returns>
		Task SetDisconnectTimeAsync(string connectionId);
	}
}
