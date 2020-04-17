using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a dispatcher for client requests to a connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantDispatcher<TRequest>
		where TRequest : IRelayClientRequest
	{
	}
}
