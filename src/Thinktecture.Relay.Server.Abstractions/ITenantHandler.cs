using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a handler processing request messages from the transport.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantHandler<out TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Event fired when an <see cref="IRelayClientRequest"/> was received.
		/// </summary>
		event AsyncEventHandler<TRequest> RequestReceived;
	}
}
