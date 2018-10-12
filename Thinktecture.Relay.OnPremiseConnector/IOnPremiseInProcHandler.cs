using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector
{
	/// <summary>
	/// Interface to be implemented by in-process request handlers in the on premise connector.
	/// </summary>
	public interface IOnPremiseInProcHandler
	{
		/// <summary>
		/// Process the request.
		/// </summary>
		/// <param name="request">The request to process.</param>
		/// <param name="response">The response to return when this request was handled.</param>
		/// <param name="cancellationToken">A cancellation token to abort the operation.</param>
		/// <returns><see cref="Task"/>.</returns>
		Task ProcessRequest(IOnPremiseTargetRequest request, IOnPremiseTargetResponse response, CancellationToken cancellationToken);
	}
}
