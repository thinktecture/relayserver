using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for acknowledgements.
	/// </summary>
	public interface IAcknowledgeCoordinator
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
		/// Acknowledges the request.
		/// </summary>
		/// <param name="request">An <see cref="IAcknowledgeRequest"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeRequestAsync(IAcknowledgeRequest request, CancellationToken cancellationToken = default);
	}
}
