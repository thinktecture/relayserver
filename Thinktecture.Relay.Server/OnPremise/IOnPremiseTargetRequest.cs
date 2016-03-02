using System.Collections.Generic;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IOnPremiseTargetRequest
	{
		string RequestId { get; }
		string HttpMethod { get; }
		string Url { get; }
		IDictionary<string, string> HttpHeaders { get; }
		string Body { get; }
		string OriginId { get; }
	}
}