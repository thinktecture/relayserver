using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of an interceptor dealing with the response of the target.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface ITargetResponseInterceptor<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Called when a response was received.
		/// </summary>
		/// <param name="context">The context of the relay task.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous interception.</returns>
		Task OnResponseReceivedAsync(IRelayContext<TRequest, TResponse> context);
	}
}
