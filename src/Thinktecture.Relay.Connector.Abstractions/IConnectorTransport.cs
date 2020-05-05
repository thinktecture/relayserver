using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// An implementation of a connector transport between connector and relay server.
	/// </summary>
	public interface IConnectorTransport
	{
		/// <summary>
		/// Send an acknowledge request to the server.
		/// </summary>
		/// <param name="request">The acknowledge request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeAsync(IAcknowledgeRequest request);

		/// <summary>
		/// Send a pong signal to the server.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task PongAsync();
	}
}
