using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for acknowledge requests.
	/// </summary>
	/// <typeparam name="T">The type of acknowledge.</typeparam>
	public interface IAcknowledgeCoordinator<in T>
		where T : IAcknowledgeRequest
	{
		/// <summary>
		/// Registers an acknowledge state.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="acknowledgeId">The id to acknowledge.</param>
		/// <param name="outsourcedRequestBodyContent">The request body content is outsourced.</param>
		void RegisterRequest(Guid requestId, string connectionId, string acknowledgeId, bool outsourcedRequestBodyContent);

		/// <summary>
		/// Processes the acknowledge request.
		/// </summary>
		/// <param name="request">The acknowledge request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task ProcessAcknowledgeAsync(T request, CancellationToken cancellationToken = default);
	}
}
