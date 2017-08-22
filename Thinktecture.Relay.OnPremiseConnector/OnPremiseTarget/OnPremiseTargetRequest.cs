using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		public string RequestId { get; set; }

		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IDictionary<string, string> HttpHeaders { get; set; }

		public byte[] Body { get; set; }

		public Guid OriginId { get; set; }

		public string AcknowledgeId { get; set; }
	}
}
