using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a handler processing request messages from the transport.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantHandler<out TRequest>
		where TRequest : IClientRequest
	{
		/// <summary>
		/// Event fired when an <see cref="IClientRequest"/> was received.
		/// </summary>
		event AsyncEventHandler<TRequest>? RequestReceived;

		/// <summary>
		/// Acknowledges an <see cref="IClientRequest"/>.
		/// </summary>
		/// <param name="acknowledgeId">The id to acknowledge.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeAsync(string acknowledgeId);
	}
}
