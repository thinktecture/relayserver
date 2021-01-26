using System;
using System.Collections.Generic;
using System.IO;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// The metadata of a client request to be relayed.
	/// </summary>
	public interface IClientRequest
	{
		/// <summary>
		/// The unique id of the request.
		/// </summary>
		/// <remarks>This should not be changed.</remarks>
		Guid RequestId { get; set; }

		/// <summary>
		/// The unique id of the origin which created the request.
		/// </summary>
		/// <remarks>This should not be changed.</remarks>
		Guid RequestOriginId { get; set; }

		/// <summary>
		/// Indicates the mode if and when the connector should acknowledge the processing of the request.
		/// </summary>
		AcknowledgeMode AcknowledgeMode { get; set; }

		/// <summary>
		/// The unique id of the origin where the acknowledgment should be send to. This will be null when <see cref="AcknowledgeMode"/> is disabled.
		/// </summary>
		/// <remarks>This should not be changed.</remarks>
		/// <seealso cref="AcknowledgeMode"/>
		Guid? AcknowledgeOriginId { get; set; }

		/// <summary>
		/// The name of the target used to request the response from.
		/// </summary>
		string Target { get; set; }

		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		Guid TenantId { get; set; }

		/// <summary>
		/// The HTTP method used by the requesting client.
		/// </summary>
		string HttpMethod { get; set; }

		/// <summary>
		/// The URL used by the requesting client relative to the <see cref="Target"/>.
		/// </summary>
		string Url { get; set; }

		/// <summary>
		/// The HTTP headers provided.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// The size of the body or null if there is no body.
		/// </summary>
		/// <seealso cref="BodyContent"/>
		long? BodySize { get; set; }

		/// <summary>
		/// The body as a <see cref="Stream"/> or null if there is no body.
		/// GET, HEAD, CONNECT, OPTIONS, DELETE must not have a body, other methods have a body.
		/// </summary>
		/// <seealso cref="BodySize"/>
		/// <remarks>Depending on the transport the stream content may be serialized inline.</remarks>
		Stream BodyContent { get; set; }

		/// <summary>
		/// Enable tracing of this particular request.
		/// </summary>
		bool EnableTracing { get; set; }
	}
}
