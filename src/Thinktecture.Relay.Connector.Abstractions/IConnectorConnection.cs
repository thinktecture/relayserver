using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// An implementation of a connection to the server.
	/// </summary>
	public interface IConnectorConnection
	{
		/// <summary>
		/// Event fired when a connection was established.
		/// </summary>
		/// <remarks>The event includes the connection id.</remarks>
		event AsyncEventHandler<string> Connected;

		/// <summary>
		/// Event fired when a connection starts to reconnect.
		/// </summary>
		/// <remarks>The event includes the connection id.</remarks>
		event AsyncEventHandler<string> Reconnecting;

		/// <summary>
		/// Event fired when a connection successfully reconnected.
		/// </summary>
		/// <remarks>The event includes the connection id.</remarks>
		event AsyncEventHandler<string> Reconnected;

		/// <summary>
		/// Event fired when a connection was disconnected.
		/// </summary>
		/// <remarks>The event includes the connection id.</remarks>
		event AsyncEventHandler<string> Disconnected;

		/// <summary>
		/// Opens a connection to the server.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ConnectAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Closes the connection to the server.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DisconnectAsync(CancellationToken cancellationToken = default);
	}
}
