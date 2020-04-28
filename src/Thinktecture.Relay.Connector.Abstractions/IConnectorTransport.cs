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
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Send an acknowledge request to the server.
		/// </summary>
		/// <param name="request">The metadata.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeAsync(IAcknowledgeRequest request);
	}
}
