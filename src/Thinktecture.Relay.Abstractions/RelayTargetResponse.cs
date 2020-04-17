using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	/// <inheritdoc />
	public class RelayTargetResponse : IRelayTargetResponse
	{
		/// <inheritdoc />
		public Guid RequestId { get; set; }
		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }
		/// <inheritdoc />
		public byte[] Body { get; set; }
		/// <inheritdoc />
		public long? BodySize { get; set; }
		/// <inheritdoc />
		public bool IsBodyAvailable { get; set; }
		/// <inheritdoc />
		public DateTime? RequestStart { get; set; }
		/// <inheritdoc />
		public TimeSpan? RequestDuration { get; set; }
	}
}
