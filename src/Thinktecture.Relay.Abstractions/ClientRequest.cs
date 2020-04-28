using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Abstractions
{
	/// <inheritdoc />
	public class ClientRequest : ITransportClientRequest
	{
		/// <inheritdoc />
		public Guid RequestId { get; set; }
		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public string TargetId { get; set; }

		/// <inheritdoc />
		public string HttpMethod { get; set; }

		/// <inheritdoc />
		public string Url { get; set; }
		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }
		/// <inheritdoc />
		byte[] ITransportClientRequest.Body { get; set; }
		/// <inheritdoc />
		public long? BodySize { get; set; }
		/// <inheritdoc />
		public bool IsBodyAvailable { get; set; }

		/// <inheritdoc />
		[JsonIgnore]
		public Stream BodyStream { get; set; }

		/// <inheritdoc />
		public AcknowledgeMode AcknowledgeMode { get; set; }
		/// <inheritdoc />
		public Guid AcknowledgeOriginId { get; set; }
	}
}
