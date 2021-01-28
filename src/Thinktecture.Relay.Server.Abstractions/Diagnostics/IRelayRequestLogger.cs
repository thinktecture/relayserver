using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Diagnostics
{
	/// <summary>
	/// An implementation of a logger for relay requests.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IRelayRequestLogger<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		/// <summary>
		/// Logs the request as succeeded.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogSuccessAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);

		/// <summary>
		/// Logs the request as aborted.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogAbortAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);

		/// <summary>
		/// Logs the request as failed.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogFailAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);

		/// <summary>
		/// Logs the request as expired.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogExpiredAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);

		/// <summary>
		/// Logs the request as erroneous.
		/// </summary>
		/// <param name="relayContext">An <see cref="IRelayContext{TRequest,TResponse}"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task LogErrorAsync(IRelayContext<TRequest, TResponse> relayContext, CancellationToken cancellationToken = default);
	}
}
