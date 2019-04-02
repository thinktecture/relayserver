using System;
using System.Net;
using Thinktecture.Relay.Server.Diagnostics;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.CustomCodeDemo
{
	public class NoopRequestLogger : IRequestLogger
	{
		public void LogRequest(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid linkId, Guid originId, String relayPath, HttpStatusCode? statusCode)
		{
			// Noop
		}
	}
}
