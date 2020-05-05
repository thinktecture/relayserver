using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a transport from and to the connector. This is the last part on the server side before the request will be
	/// transported to the connector, thus this is the real transport implementation (e.g. SignalR) from the server to the connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface IConnectorTransport<in TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Request the response from a target.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTargetAsync(TRequest request, string connectionId, CancellationToken cancellationToken = default);
	}
}
