using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a handler processing request messages from the transport.
	/// </summary>
	public interface ITenantHandler
	{
		/// <summary>
		/// Acknowledges a client request.
		/// </summary>
		/// <param name="acknowledgeId">The unique id of the acknowledge.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeAsync(string acknowledgeId, CancellationToken cancellationToken = default);
	}
}
