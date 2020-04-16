using System;

namespace Thinktecture.Relay.Abstractions
{
	public interface IRelayRequest : IRelayTask
	{
		/// <summary>
		/// The HTTP method.
		/// </summary>
		string HttpMethod { get; set; }

		/// <summary>
		/// The URL.
		/// </summary>
		string Url { get; set; }

		AcknowledgeMode AcknowledgeMode { get; set; }

		/// <summary>
		/// The unique id of the server which received the request.
		/// </summary>
		Guid AcknowledgeOriginId { get; set; }
	}
}
