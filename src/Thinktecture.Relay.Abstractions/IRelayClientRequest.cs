using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.Abstractions
{
	/// <summary>
	/// The metadata of a client request to be relayed.
	/// </summary>
	public interface IRelayClientRequest
	{
		/// <summary>
		/// The unique id of the request.
		/// <remarks>This should not be changed.</remarks>
		/// </summary>
		Guid RequestId { get; set; }

		/// <summary>
		/// The unique id of the server which created the request.
		/// <remarks>This should not be changed.</remarks>
		/// </summary>
		Guid RequestOriginId { get; set; }

		/// <summary>
		/// The name of the target used to request the response from.
		/// </summary>
		string Target { get; set; }

		/// <summary>
		/// The HTTP method used by the requesting client.
		/// </summary>
		string HttpMethod { get; set; }

		/// <summary>
		/// The URL used by the requesting client relative to the <see cref="Target"/>.
		/// </summary>
		string Url { get; set; }

		/// <summary>
		/// The HTTP headers provided by the requesting client.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// The size of an optional body provided by the requesting client.
		/// <remarks>A value of <value>null</value> means that the size is unknown.</remarks>
		/// <seealso cref="IsBodyAvailable"/>
		/// </summary>
		long? BodySize { get; set; }

		/// <summary>
		/// Indicates if a body was provided by the requesting client. If false the value of <see cref="BodyStream"/> and
		/// <see cref="BodySize"/> should be ignored.
		/// </summary>
		bool IsBodyAvailable { get; set; }

		/// <summary>
		/// The body as a stream.
		/// </summary>
		Stream BodyStream { get; set; }

		/// <summary>
		/// Indicating the mode if and when the connector should acknowledge the processing of the request.
		/// </summary>
		AcknowledgeMode AcknowledgeMode { get; set; }

		/// <summary>
		/// The unique id of the server where the acknowledgment should be send to.
		/// </summary>
		Guid? AcknowledgeOriginId { get; set; }
	}
}
