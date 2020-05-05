using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Transport
{
	/// <inheritdoc />
	public class TargetResponse : IRelayTargetResponse
	{
		/// <inheritdoc />
		public Guid RequestId { get; set; }

		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public DateTime? RequestStart { get; set; }

		/// <inheritdoc />
		public TimeSpan? RequestDuration { get; set; }

		/// <inheritdoc />
		public HttpStatusCode HttpStatusCode { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <inheritdoc />
		public long? BodySize { get; set; }

		/// <inheritdoc />
		[JsonConverter(typeof(InlineMemoryStreamJsonConverter))]
		public Stream BodyContent { get; set; }
	}
}
