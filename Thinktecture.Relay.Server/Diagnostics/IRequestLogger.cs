using System;
using System.Net;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface IRequestLogger
	{
		void LogRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, Guid originId, string relayPath, HttpStatusCode statusCode);
	}
}
