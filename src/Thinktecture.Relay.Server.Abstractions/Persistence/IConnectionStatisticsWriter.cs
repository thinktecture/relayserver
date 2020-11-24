using System;
using System.Net;
using System.Threading;
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
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the last activity of a connection.
		/// </summary>
		/// <param name="connectionId">The id of the connection that showed an activity.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the information that a connection was shut down.
		/// </summary>
		/// <param name="connectionId">The id of the connection to mark as stopped.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default);
	}
}
