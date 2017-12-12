using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Thinktecture.Relay.Server.OnPremise
{
	internal class OnPremiseConnectorRequest : IOnPremiseConnectorRequest
	{
		public string RequestId { get; set; }
		public Guid OriginId { get; set; }
		public string AcknowledgeId { get; set; }
		public DateTime RequestStarted { get; set; }
		public DateTime RequestFinished { get; set; }
		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		public byte[] Body { get; set; }

		[JsonIgnore]
		public Stream Stream { get; set; }

		[JsonIgnore]
		public long ContentLength { get; set; }

		[JsonIgnore]
		public bool SendToOnPremiseConnector { get; set; }
	}
}
