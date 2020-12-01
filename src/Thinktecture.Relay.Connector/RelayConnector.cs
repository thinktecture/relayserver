using System;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// A connection to a RelayServer.
	/// </summary>
	/// <remarks>This is just a convenient class for holding the transient <see cref="IConnectorConnection"/>.</remarks>
	public class RelayConnector
	{
		private readonly IConnectorConnection _connection;

		/// <summary>
		/// Initializes a new instance of the <see cref="RelayConnector"/> class.
		/// </summary>
		/// <param name="connection">An <see cref="IConnectorConnection"/>.</param>
		public RelayConnector(IConnectorConnection connection)
			=> _connection = connection ?? throw new ArgumentNullException(nameof(connection));

		/// <summary>
		/// Opens a connection to the server.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task ConnectAsync(CancellationToken cancellationToken = default) => _connection.ConnectAsync(cancellationToken);

		/// <summary>
		/// Closes the connection to the server.
		/// </summary>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task DisconnectAsync(CancellationToken cancellationToken = default) => _connection.DisconnectAsync(cancellationToken);
	}
}
