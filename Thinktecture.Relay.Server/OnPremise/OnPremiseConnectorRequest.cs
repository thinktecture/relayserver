using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseConnectorRequest : IOnPremiseConnectorRequest
	{
		public string RequestId { get; set; }

		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }

		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }

		public string OriginId { get; set; }
	}
}
