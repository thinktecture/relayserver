using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Adapter that allows writing statistics for connections.
/// </summary>
/// <remarks>
/// This class should always be registered as a singleton, because it is creating an own scope during the
/// execution of any method. It also buffers data for batched and delayed writing.
/// </remarks>
public interface IConnectionStatisticsWriter
{
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
	/// Updates the last seen time of a connection. This may cache or buffer the updates and does not need
	/// to immediately write these to the persistence layer.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task UpdateLastSeenTimeAsync(string connectionId, CancellationToken cancellationToken = default);

	/// <summary>
	/// This method is called regularly and writes / flushes all last seen values to the persistence layer.
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task WriteLastSeenEntriesAsync(CancellationToken cancellationToken = default);

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
}
