using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets;

/// <summary>
/// An implementation of a client request handler for handling the request in the first step.
/// </summary>
/// <typeparam name="T">The type of request.</typeparam>
/// <seealso cref="IClientRequestWorker{TRequest,TResponse}"/>
public interface IClientRequestHandler<in T>
	where T : IClientRequest
{
    /// <summary>
	/// Handles the request.
	/// </summary>
	/// <param name="request">The client request.</param>
	/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task HandleAsync(T request, CancellationToken cancellationToken = default);

	/// <summary>
	/// Acknowledges the request.
	/// </summary>
	/// <param name="request">The client request.</param>
	/// <param name="removeRequestBodyContent">Indicates if the request body content should be removed.</param>
	/// <returns></returns>
	Task AcknowledgeRequestAsync(T request, bool removeRequestBodyContent);
}
