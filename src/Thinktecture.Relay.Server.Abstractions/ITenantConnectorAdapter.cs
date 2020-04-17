using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of an adapter between a tenant and a connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantConnectorAdapter<TRequest>
		where TRequest : IRelayClientRequest
	{
	}
}
