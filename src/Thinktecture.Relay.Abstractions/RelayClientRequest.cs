using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	public class RelayClientRequest : IRelayClientRequest
	{
		public Guid RequestId { get; set; }
		public Guid RequestOriginId { get; set; }

		public string HttpMethod { get; set; }
		public string Url { get; set; }
		public IDictionary<string, string[]> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public long? BodySize { get; set; }
		public bool IsBodyAvailable { get; set; }

		public AcknowledgeMode AcknowledgeMode { get; set; }
		public Guid AcknowledgeOriginId { get; set; }
	}
}
