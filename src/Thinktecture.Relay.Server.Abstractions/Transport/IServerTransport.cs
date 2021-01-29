using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a dispatcher for server to server messages.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	/// <typeparam name="TAcknowledge">The type of acknowledge.</typeparam>
	public interface IServerTransport<in TResponse, in TAcknowledge>
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		/// <summary>
		/// The maximum size of binary data the protocol is capable to serialize inline, or null if there is no limit.
		/// </summary>
		int? BinarySizeThreshold { get; }

		/// <summary>
		/// Dispatches the target response to the origin server.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchResponseAsync(TResponse response);

		/// <summary>
		/// Dispatches the acknowledge request to the origin server.
		/// </summary>
		/// <param name="request">The acknowledge request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchAcknowledgeAsync(TAcknowledge request);
	}
}
