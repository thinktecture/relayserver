using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Transport
{
	/// <inheritdoc />
	public class ClientRequest : IClientRequest
	{
		/// <inheritdoc />
		public Guid RequestId { get; set; }

		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public AcknowledgeMode AcknowledgeMode { get; set; }

		/// <inheritdoc />
		public Guid? AcknowledgeOriginId { get; set; }

		/// <inheritdoc />
		public string Target { get; set; } = default!;

		/// <inheritdoc />
		public Guid TenantId { get; set; }

		/// <inheritdoc />
		public string HttpMethod { get; set; } = default!;

		/// <inheritdoc />
		public string Url { get; set; } = default!;

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; } = default!;

		/// <inheritdoc />
		public long? BodySize { get; set; }

		/// <inheritdoc />
		[JsonConverter(typeof(InlineMemoryStreamJsonConverter))]
		public Stream? BodyContent { get; set; }

		/// <inheritdoc />
		public bool EnableTracing { get; set; }
	}
}
