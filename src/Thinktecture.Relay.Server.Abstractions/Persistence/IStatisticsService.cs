using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Represents a way to access statistics data in the persistence layer.
/// </summary>
public interface IStatisticsService
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

	/// <summary>
	/// Cleans up stale origins older than the specified timespan.
	/// </summary>
	/// <param name="maxAge">The time span in which to still keep old entries.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task CleanUpOriginsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the connection time of a connection.
	/// </summary>
	/// <param name="connectionId">The connection id from the corresponding transport.</param>
	/// <param name="tenantId">The unique id of the tenant.</param>
	/// <param name="originId">The unique id of the origin.</param>
	/// <param name="remoteIpAddress">The optional remote ip address of the connection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SetConnectionTimeAsync(string connectionId, Guid tenantId, Guid originId, IPAddress? remoteIpAddress,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// Sets the disconnect time of a connection.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task SetDisconnectTimeAsync(string connectionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// Cleans up stale connections older than the specified timespan.
	/// </summary>
	/// <param name="maxAge">The time span in which to still keep old entries.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task CleanUpConnectionsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default);

	/// <summary>
	/// Updates the last seen time of several connections.
	/// </summary>
	/// <param name="data">A list of connection ids and their corresponding last seen time to update.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task UpdateLastSeenTimeAsync(IDictionary<string, DateTimeOffset> data, CancellationToken cancellationToken = default);
}
