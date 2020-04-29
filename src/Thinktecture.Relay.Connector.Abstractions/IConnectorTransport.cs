using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Connector.Abstractions
{
	/// <summary>
	/// An implementation of a connector transport between connector and relay server.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IConnectorTransport<TRequest, TResponse>
		where TRequest : ITransportClientRequest
		where TResponse : ITransportTargetResponse
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

		/// <summary>
		/// Send the target response to the server.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DeliverAsync(ITransportTargetResponse response);
	}
}
