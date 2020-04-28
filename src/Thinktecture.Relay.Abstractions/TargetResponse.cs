using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Abstractions
{
	/// <inheritdoc />
	public class TargetResponse : ITransportTargetResponse
	{
		/// <inheritdoc />
		public Guid RequestId { get; set; }
		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }
		/// <inheritdoc />
		byte[] ITransportTargetResponse.Body { get; set; }
		/// <inheritdoc />
		public long? BodySize { get; set; }
		/// <inheritdoc />
		public bool IsBodyAvailable { get; set; }

		/// <inheritdoc />
		[JsonIgnore]
		public Stream BodyStream { get; set; }

		/// <inheritdoc />
		public DateTime? RequestStart { get; set; }
		/// <inheritdoc />
		public TimeSpan? RequestDuration { get; set; }
	}
}
