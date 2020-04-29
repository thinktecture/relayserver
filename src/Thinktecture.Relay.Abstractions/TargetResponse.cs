using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;

namespace Thinktecture.Relay.Abstractions
{
	/// <inheritdoc />
	public class TargetResponse : ITransportTargetResponse
	{
		private byte[] _body;
		private Stream _bodyStream;

		/// <inheritdoc />
		public Guid RequestId { get; set; }

		/// <inheritdoc />
		public Guid RequestOriginId { get; set; }

		/// <inheritdoc />
		public IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <inheritdoc />
		byte[] ITransportTargetResponse.Body
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
		public DateTime? RequestStart { get; set; }

		/// <inheritdoc />
		public TimeSpan? RequestDuration { get; set; }
	}
}
