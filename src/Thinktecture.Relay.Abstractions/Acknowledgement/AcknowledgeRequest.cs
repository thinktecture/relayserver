using System;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <inheritdoc />
	public class AcknowledgeRequest : IAcknowledgeRequest
	{
		/// <inheritdoc />
		public Guid OriginId { get; set; }

		/// <inheritdoc />
		public Guid RequestId { get; set; }

		/// <inheritdoc />
		public bool RemoveRequestBodyContent { get; set; }
	}
}
