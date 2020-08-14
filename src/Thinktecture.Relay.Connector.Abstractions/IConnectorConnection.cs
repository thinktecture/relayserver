using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// Implement a connection to the server.
	/// </summary>
	public interface IConnectorConnection
	{
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
