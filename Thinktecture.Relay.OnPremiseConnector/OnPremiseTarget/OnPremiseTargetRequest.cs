using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		public string RequestId { get; set; }

		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IDictionary<string, string> HttpHeaders { get; set; }
		IReadOnlyDictionary<string, string> IOnPremiseTargetRequest.HttpHeaders => new ReadOnlyDictionary<string, string>(HttpHeaders);

		public byte[] Body { get; set; }

		public Guid OriginId { get; set; }

		public string AcknowledgeId { get; set; }
	}
}
