using System;

namespace Thinktecture.Relay.Acknowledgement
{
	/// <summary>
	/// The metadata of an acknowledgement.
	/// </summary>
	public interface IAcknowledgeRequest
	{
		/// <summary>
		/// The unique id of the origin where the acknowledgement should be send to.
		/// </summary>
		Guid OriginId { get; set; }

		/// <summary>
		/// The unique id of the request.
		/// </summary>
		Guid RequestId { get; set; }

		/// <summary>
		/// Indicates if the request body content should be removed.
		/// </summary>
		bool RemoveRequestBodyContent { get; set; }
	}
}
