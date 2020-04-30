using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a handler processing server to server messages.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IServerHandler<TResponse>
		where TResponse : ITransportTargetResponse
	{
		// TODO methods/events/tbd for consuming messages
	}
}
