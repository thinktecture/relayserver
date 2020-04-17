using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a transport from and to the connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface IConnectorTransport<TRequest>
		where TRequest : IRelayClientRequest
	{
	}
}
