using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// An implementation of a connector transport between connector and relay server.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IConnectorTransport<in TResponse>
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// The maximum size of binary data the protocol is capable to serialize inline, or null if there is no limit.
		/// </summary>
		int? BinarySizeThreshold { get; }

		/// <summary>
		/// Send the target response to the server.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeliverAsync(TResponse response);

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
