using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Diagnostics;

/// <summary>
/// An implementation of a logger for relay requests.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IRelayRequestLogger<TRequest, in TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	/// <summary>
	/// Logs the request as succeeded.
	/// </summary>
	/// <param name="relayContext">An <see cref="IRelayContext"/>.</param>
	/// <param name="bodySize">The size of the current request body.</param>
	/// <param name="httpRequest">A <see cref="HttpRequest"/>.</param>
	/// <param name="targetResponse">An optional <see cref="ITargetResponse"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogSuccessAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest,
		TResponse? targetResponse);

	/// <summary>
	/// Logs the request as aborted.
	/// </summary>
	/// <param name="relayContext">An <see cref="IRelayContext"/>.</param>
	/// <param name="bodySize">The size of the current request body.</param>
	/// <param name="httpRequest">A <see cref="HttpRequest"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogAbortAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest);

	/// <summary>
	/// Logs the request as failed.
	/// </summary>
	/// <param name="relayContext">An <see cref="IRelayContext"/>.</param>
	/// <param name="bodySize">The size of the current request body.</param>
	/// <param name="httpRequest">A <see cref="HttpRequest"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogFailAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest);

	/// <summary>
	/// Logs the request as expired.
	/// </summary>
	/// <param name="relayContext">An <see cref="IRelayContext"/>.</param>
	/// <param name="bodySize">The size of the current request body.</param>
	/// <param name="httpRequest">A <see cref="HttpRequest"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogExpiredAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest);

	/// <summary>
	/// Logs the request as erroneous.
	/// </summary>
	/// <param name="relayContext">An <see cref="IRelayContext"/>.</param>
	/// <param name="bodySize">The size of the current request body.</param>
	/// <param name="httpRequest">A <see cref="HttpRequest"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task LogErrorAsync(IRelayContext relayContext, long bodySize, HttpRequest httpRequest);
}
