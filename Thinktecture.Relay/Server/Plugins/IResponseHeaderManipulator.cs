using System.Collections.Generic;
using System.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// Interface for plugins that are capable of modifying the header of an outgoing response after it was received from the remote destination.
	/// </summary>
	public interface IResponseHeaderManipulator
	{
		/// <summary>
		/// This method can replace the headers of a response if it returns a new header collection to replace the old.
		/// </summary>
		/// <param name="response">May be null! The response from the remote location that is to be modified, or null if no response could be retrieved.</param>
		/// <param name="request">The original request that the <paramref name="response"/> is meant for.</param>
		/// <param name="immidiateResponse">If set to something different than null, the provided <see cref="HttpResponseMessage"/> will immidiately be send out as a response for the provided <paramref name="request"/>.</param>
		/// <returns>Return the new headers that will replace, not extend, the headers on the provided <paramref name="response"/>.
		/// Return null, if no action should be taken.</returns>
		IDictionary<string, string> HandleHeaders(IOnPremiseTargetResponse response, IOnPremiseConnectorRequest request, out HttpResponseMessage immidiateResponse);
	}
}
