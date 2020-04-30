using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a dispatcher for server to server messages.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IServerDispatcher<in TResponse>
		where TResponse : ITransportTargetResponse
	{
		/// <summary>
		/// Dispatches the response to the origin server.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchResponseAsync(TResponse response);

		/// <summary>
		/// Dispatches the acknowledgement to the origin server.
		/// </summary>
		/// <param name="request">The acknowledge request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchAcknowledgeAsync(IAcknowledgeRequest request);
	}
}
