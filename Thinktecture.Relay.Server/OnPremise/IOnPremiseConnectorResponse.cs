using System.Collections.Generic;
using System.Net;
using Thinktecture.Relay.OnPremiseConnector;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseConnectorResponse : IOnPremiseTargetResponse
	{
		new IDictionary<string, string> HttpHeaders { get; }
		new HttpStatusCode StatusCode { get; set; }
	}
}
