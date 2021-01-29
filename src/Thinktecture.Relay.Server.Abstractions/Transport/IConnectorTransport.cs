using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a transport for requests.
	/// </summary>
	/// <typeparam name="T">The type of request.</typeparam>
	public interface IConnectorTransport<in T>
		where T : IClientRequest
	{
		/// <summary>
		/// Transports the client request.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task TransportAsync(T request, CancellationToken cancellationToken = default);
	}
}
