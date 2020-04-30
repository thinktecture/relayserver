using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Abstractions
{
	/// <inheritdoc />
	public class ClientRequest : ITransportClientRequest
	{
		private byte[] _body;
		private Stream _bodyStream;

		/// <inheritdoc />
		public Guid RequestId { get; set; }

		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public string Target { get; set; }

		/// <inheritdoc />
		public string HttpMethod { get; set; }

		/// <inheritdoc />
		public string Url { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <inheritdoc />
		byte[] ITransportClientRequest.Body
		{
			get => _body;
			set
			{
				_body = value;
				if (value != null)
				{
					_bodyStream = null;
				}
			}
		}

		/// <inheritdoc />
		public long? BodySize { get; set; }

		/// <inheritdoc />
		public bool IsBodyAvailable { get; set; }

		/// <inheritdoc />
		[JsonIgnore]
		public Stream BodyStream
		{
			get => _bodyStream;
			set
			{
				_bodyStream = value;
				if (value != null)
				{
					_body = null;
				}
			}
		}

		/// <inheritdoc />
		public AcknowledgeMode AcknowledgeMode { get; set; }

		/// <inheritdoc />
		public Guid? AcknowledgeOriginId { get; set; }
	}
}
