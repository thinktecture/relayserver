using System;

namespace Thinktecture.Relay.Abstractions
{
	/// <summary>
	/// The metadata of an acknowledgement.
	/// </summary>
	public interface IAcknowledgeRequest
	{
		/// <summary>
		/// The unique id of the mesaage.
		/// </summary>
		string AcknowledgeId { get; set; }

		/// <summary>
		/// The unique id of the connection.
		/// </summary>
		string ConnectionId { get; set; }

		/// <summary>
		/// The unique id of the server where the acknowledgment should be send to.
		/// </summary>
		Guid OriginId { get; set; }
	}
}
