using System;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <inheritdoc />
	public class AcknowledgeRequest : IAcknowledgeRequest
	{
		/// <inheritdoc />
		public string AcknowledgeId { get; set; }

		/// <inheritdoc />
		public string ConnectionId { get; set; }

		/// <inheritdoc />
		public Guid OriginId { get; set; }
	}
}
