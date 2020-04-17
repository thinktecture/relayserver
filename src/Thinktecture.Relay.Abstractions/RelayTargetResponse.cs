using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	public class RelayTargetResponse : IRelayTargetResponse
	{
		public Guid RequestId { get; set; }
		public Guid RequestOriginId { get; set; }

		public IDictionary<string, string[]> HttpHeaders { get; set; }
		public byte[] Body { get; set; }
		public long? BodySize { get; set; }
		public bool IsBodyAvailable { get; set; }
		public DateTime? RequestStart { get; set; }
		public TimeSpan? RequestDuration { get; set; }
	}
}
