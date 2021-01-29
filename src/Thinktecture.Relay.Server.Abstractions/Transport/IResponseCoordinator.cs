using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for responses.
	/// </summary>
	/// <typeparam name="T">The type of response.</typeparam>
	public interface IResponseCoordinator<T>
		where T : ITargetResponse
	{
		/// <summary>
		/// Registers the request for coordination.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <returns>An <see cref="IAsyncDisposable"/> which has to be disposed when the target response should be discarded.</returns>
		IAsyncDisposable RegisterRequest(Guid requestId);

		/// <summary>
		/// Gets the target response for the client request.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the <see cref="IResponseContext{T}"/>.</returns>
		Task<IResponseContext<T>?> GetResponseAsync(Guid requestId, CancellationToken cancellationToken = default);

		/// <summary>
		/// Processes the target response.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ProcessResponseAsync(T response, CancellationToken cancellationToken = default);
	}
}
