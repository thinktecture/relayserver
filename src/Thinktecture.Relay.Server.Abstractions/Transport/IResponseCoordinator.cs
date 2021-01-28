using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for responses.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IResponseCoordinator<TResponse>
		where TResponse : class, ITargetResponse
	{
		/// <summary>
		/// Registers the request for coordination.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <returns>An <see cref="IAsyncDisposable"/> which has to be disposed when the response should be discarded.</returns>
		IAsyncDisposable RegisterRequest(Guid requestId);

		/// <summary>
		/// Gets the response for the request.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="IResponseContext{TResponse}"/>.</returns>
		Task<IResponseContext<TResponse>?> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Processes the response.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ProcessResponseAsync(TResponse response);
	}
}
