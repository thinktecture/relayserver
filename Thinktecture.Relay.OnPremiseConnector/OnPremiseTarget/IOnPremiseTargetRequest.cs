using System.Collections.Generic;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetRequest
	{
		string RequestId { get; set; }
		string HttpMethod { get; set; }
		string Url { get; set; }
		IDictionary<string, string> HttpHeaders { get; set; }
		byte[] Body { get; set; }
		string OriginId { get; set; }
	}
}