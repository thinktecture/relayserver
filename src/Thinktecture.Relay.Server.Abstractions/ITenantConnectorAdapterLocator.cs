using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a locator for an <see cref="ITenantConnectorAdapter{TRequest}"/>.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantConnectorAdapterLocator<TRequest>
		where TRequest : IRelayClientRequest
	{
	}
}
