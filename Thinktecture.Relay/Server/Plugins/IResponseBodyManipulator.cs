using System.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// Interface for plugins that are capable of modifying the body of an outgoing response after it was received from the remote destination.
	/// </summary>
	public interface IResponseBodyManipulator
	{
		/// <summary>
		/// This method can replace the body of a response if it returns a new body to replace the old.
		/// </summary>
		/// <param name="response">May be null! The response from the remote location that is to be modified, or null if no response could be retrieved.</param>
		/// <param name="request">The original request that the <paramref name="response"/> is meant for.</param>
		/// <param name="immidiateResponse">If set to something different than null, the provided <see cref="HttpResponseMessage"/> will immidiately be send out as a response for the provided <paramref name="request"/>.</param>
		/// <returns>Return the new body that will replace the body on the provided <paramref name="response"/>.
		/// Return null, if no action should be taken.</returns>
		byte[] HandleBody(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);
	}
}
