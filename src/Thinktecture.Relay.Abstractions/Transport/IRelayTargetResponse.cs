using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// The metadata of a target response to be relayed.
	/// </summary>
	public interface IRelayTargetResponse
	{
		/// <summary>
		/// The unique id of the request.
		/// </summary>
		/// <remarks>This should not be changed.</remarks>
		Guid RequestId { get; set; }

		/// <summary>
		/// The unique id of the server which created the request.
		/// </summary>
		/// <remarks>This should not be changed.</remarks>
		Guid RequestOriginId { get; set; }

		/// <summary>
		/// The time when the target was requested in behalf.
		/// </summary>
		/// <remarks>This will only be set when tracing is enabled.</remarks>
		DateTime? RequestStart { get; set; }

		/// <summary>
		/// The duration until the target returned its results.
		/// </summary>
		/// <remarks>This will only be set when tracing is enabled.</remarks>
		TimeSpan? RequestDuration { get; set; }

		/// <summary>
		/// The <see cref="HttpStatusCode"/> received from the target.
		/// </summary>
		HttpStatusCode HttpStatusCode { get; set; }

		/// <summary>
		/// The HTTP headers provided.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// The size of the body or null if the size is unknown.
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
	}
}
