using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for responses.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IResponseCoordinator<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Gets the response for the request.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="ITargetResponse"/>.</returns>
		Task<TResponse> GetResponseAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);

		/// <summary>
		/// Processes the response.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ProcessResponseAsync(TResponse response);
	}
}
