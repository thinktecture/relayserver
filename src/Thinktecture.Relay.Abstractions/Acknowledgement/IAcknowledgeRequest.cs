using System;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <summary>
	/// The metadata of an acknowledgement.
	/// </summary>
	public interface IAcknowledgeRequest
	{
		/// <summary>
		/// The unique id of the server where the acknowledgment should be send to.
		/// </summary>
		Guid OriginId { get; set; }

		/// <summary>
		/// The unique id of the message.
		/// </summary>
		Guid RequestId { get; set; }

		/// <summary>
		/// The request body content should be removed.
		/// </summary>
		bool RemoveRequestBodyContent { get; set; }
	}
}
