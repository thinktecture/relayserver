using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	public class RelayResponse : IRelayResponse
	{
		public Guid RequestId { get; set; }
		public Guid RequestOriginId { get; set; }

		public IDictionary<string, string[]> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public long? BodySize { get; set; }
		public bool IsBodyAvailable { get; set; }

		public DateTime? TargetStart { get; set; }
		public TimeSpan? TargetDuration { get; set; }
	}
}
