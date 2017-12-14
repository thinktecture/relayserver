using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal class OnPremiseTargetRequest : IOnPremiseTargetRequestInternal
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }
	}
}
