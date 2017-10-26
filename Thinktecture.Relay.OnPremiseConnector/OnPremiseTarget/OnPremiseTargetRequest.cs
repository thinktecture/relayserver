using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequest
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public Stream Stream { get; set; } = Stream.Null;
	}
}
