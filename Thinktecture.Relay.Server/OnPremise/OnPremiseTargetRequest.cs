using System.Collections.Generic;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		public string RequestId { get; set; }

		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IDictionary<string, string> HttpHeaders { get; set; }

		public string Body { get; set; } // needs to be pre-converted to base64 (because SignalR does not support byte arrays)

		public string OriginId { get; set; }
	}
}
