using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <summary>
/// An implementation of a writer for <see cref="ITargetResponse"/> to <see cref="HttpResponse"/>.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public interface IRelayTargetResponseWriter<in TRequest, in TResponse>
	where TRequest : IClientRequest
	where TResponse : class, ITargetResponse
{
	/// <summary>
	/// Writes the target response to the <see cref="HttpResponse"/>.
	/// </summary>
	/// <param name="clientRequest">An <see cref="IClientRequest"/>.</param>
	/// <param name="targetResponse">An <see cref="ITargetResponse"/>.</param>
	/// <param name="httpResponse">The <see cref="HttpResponse"/>.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task WriteAsync(TRequest clientRequest, TResponse? targetResponse, HttpResponse httpResponse,
		CancellationToken cancellationToken = default);
}
