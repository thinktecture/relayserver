using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Interceptor
{
	/// <summary>
	/// An implementation of an interceptor dealing with the request of the client.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IClientRequestInterceptor<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		/// <summary>
		/// Called when a request was received.
		/// </summary>
		/// <param name="context">The context of the relay task.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task OnRequestReceivedAsync(IRelayContext<TRequest, TResponse> context, CancellationToken cancellationToken = default);
	}
}
