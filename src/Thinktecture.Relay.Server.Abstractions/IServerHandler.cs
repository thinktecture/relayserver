using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a handler processing server to server messages.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IServerHandler<TResponse>
		where TResponse : IRelayTargetResponse
	{
	}
}
