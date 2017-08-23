﻿using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// Interface for plugins that are capable of modifying the method of an incoming request before it gets relayed to its destination.
	/// </summary>
	public interface IRequestMethodManipulator
	{
		/// <summary>
		/// This method can replace the http method of a request if it returns a new method to replace the old.
		/// </summary>
		/// <param name="request">The request that is about to be modified.</param>
		/// <param name="response">If set to something different than null, the provided <see cref="HttpResponseMessage"/> will immidiately be send out as a response for the provided <paramref name="request"/>.</param>
		/// <returns>Return the new method that will replace the method on the provided <paramref name="request"/>.
		/// Return null, if no action should be taken.</returns>
		string HandleMethod(IOnPremiseConnectorRequest request, out HttpResponseMessage response);
	}
}
