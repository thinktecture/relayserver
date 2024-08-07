using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Connector.Transport;

/// <summary>
/// An implementation of a transport for acknowledge requests.
/// </summary>
/// <typeparam name="T">The type of acknowledge.</typeparam>
public interface IAcknowledgeTransport<in T>
	where T : IAcknowledgeRequest
{
	/// <summary>
	/// Transports the acknowledge request.
	/// </summary>
	/// <param name="request">The acknowledge request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task TransportAsync(T request, CancellationToken cancellationToken = default);
}
