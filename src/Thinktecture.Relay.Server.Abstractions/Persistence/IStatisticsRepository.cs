using System;
using System.Net;
using System.Threading;
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
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task SetStartupTimeAsync(Guid originId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the last seen time stamp of an origin statistics entry.
		/// </summary>
		/// <param name="originId">The id of the origin to update.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task UpdateLastSeenTimeAsync(Guid originId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the statistic entry of an origin when it shuts down.
		/// </summary>
		/// <param name="originId">The id of the origin to mark as stopped.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task SetShutdownTimeAsync(Guid originId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Cleans up stale origins older than the specified timespan.
		/// </summary>
		/// <param name="maxAge">The time span in which to still keep old entries.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task CleanUpOriginsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

		/// <summary>
		/// Creates a new statistics entry for a connection.
		/// </summary>
		/// <param name="connectionId">The connection id from the corresponding transport.</param>
		/// <param name="tenantId">The id of the tenant this connection is created for.</param>
		/// <param name="originId">The id of the server this connection is created to.</param>
		/// <param name="remoteIpAddress">The remote ip address that initiated this connection.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress remoteIpAddress, CancellationToken cancellationToken = default);

		/// <summary>
		/// Updates the last activity of a connection.
		/// </summary>
		/// <param name="connectionId">The id of the connection that showed an activity.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task UpdateLastActivityTimeAsync(string connectionId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Writes the information that a connection was shut down.
		/// </summary>
		/// <param name="connectionId">The id of the connection to mark as stopped.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Cleans up stale connections older than the specified timespan.
		/// </summary>
		/// <param name="maxAge">The time span in which to still keep old entries.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns></returns>
		Task CleanUpConnectionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);
	}
}
