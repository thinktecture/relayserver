using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	public interface IOnPremiseTargetReponse
	{
		string RequestId { get; }
		string OriginId { get; }
		IDictionary<string, string> HttpHeaders { get; }
		HttpStatusCode StatusCode { get; }
		byte[] Body { get; }
		DateTime RequestStarted { get; }
		DateTime RequestFinished { get; }
	}
}