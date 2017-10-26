using System.Collections.Generic;
using Thinktecture.Relay.OnPremiseConnector;

namespace Thinktecture.Relay.Server.OnPremise
{
	public interface IWriteableOnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		new string HttpMethod { get; set; }
		new string Url { get; set; }
		new IDictionary<string, string> HttpHeaders { get; }
		new byte[] Body { get; set; }
	}
}
