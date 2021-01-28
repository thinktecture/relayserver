using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <summary>
	/// A client request handler determining the <see cref="IRelayTarget{TRequest,TResponse}"/> handling the request.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IClientRequestHandler<in TRequest, out TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Event fired when a response should be delivered.
		/// </summary>
		event AsyncEventHandler<TResponse>? DeliverResponse;

		/// <summary>
		/// Event fired when a request should be acknowledged.
		/// </summary>
		event AsyncEventHandler<IAcknowledgeRequest>? AcknowledgeRequest;

		/// <summary>
		/// Limits the number of background tasks running in parallel or null for system's recommended limit.
		/// </summary>
		/// <remarks>This cannot be smaller than the number of processors on the computer.</remarks>
		int? BackgroundTaskLimit { get; set; }

		/// <summary>
		/// Handles the request.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="binarySizeThreshold">The maximum size of binary data the protocol is capable to serialize inline, or null if there is no limit.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task HandleAsync(TRequest request, int? binarySizeThreshold, CancellationToken cancellationToken = default);

		/// <summary>
		/// Acknowledges the request.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="removeRequestBodyContent">Indicates if the request body content should be removed.</param>
		/// <returns></returns>
		Task AcknowledgeRequestAsync(TRequest request, bool removeRequestBodyContent);
	}
}
