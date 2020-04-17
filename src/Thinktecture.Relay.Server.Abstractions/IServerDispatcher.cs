using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a dispatcher for server to server messages.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IServerDispatcher<TResponse>
		where TResponse : IRelayTargetResponse
	{
	}
}
