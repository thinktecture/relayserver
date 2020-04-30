using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of an adapter between a tenant and a connector. This consumes the messages from the underlying transport.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface ITenantConnectorAdapter<TRequest, TResponse>
		where TRequest : ITransportClientRequest
		where TResponse : ITransportTargetResponse
	{
		// TODO methods/events/tbd for consuming messages
	}
}
