using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorRequest
	{
		string RequestId { get; }
		string HttpMethod { get; }
		string Url { get; }
		IDictionary<string, string> HttpHeaders { get; }
		byte[] Body { get; }
		DateTime RequestStarted { get; }
		DateTime RequestFinished { get; }
		string OriginId { get; }
	}
}