using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// A client request handler determining the <see cref="IRelayTarget{TRequest,TResponse}"/> handling the request.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IClientRequestHandler<in TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Event fired when a request should be acknowledged.
		/// </summary>
		event AsyncEventHandler<IAcknowledgeRequest> Acknowledge;

		/// <summary>
		/// Called when a request was received.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="binarySizeThreshold">The maximum size of binary data the protocol is capable to serialize inline, or null if there is no limit.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		Task<TResponse> HandleAsync(TRequest request, int? binarySizeThreshold, CancellationToken cancellationToken = default);
	}
}
