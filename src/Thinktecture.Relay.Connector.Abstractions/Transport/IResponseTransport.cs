using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Transport;

/// <summary>
/// An implementation of a transport for responses.
/// </summary>
/// <typeparam name="T">The type of response.</typeparam>
public interface IResponseTransport<in T>
	where T : ITargetResponse
{
	/// <summary>
	/// Transports the target response.
	/// </summary>
	/// <param name="response">The target response.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task TransportAsync(T response, CancellationToken cancellationToken = default);
}
