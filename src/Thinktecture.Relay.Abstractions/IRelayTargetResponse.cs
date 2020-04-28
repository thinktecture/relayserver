using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.Abstractions
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
		/// The HTTP headers returned by the requested target.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// The size of an optional body returned by the requested target.
		/// </summary>
		/// <seealso cref="IsBodyAvailable"/>
		/// <remarks>A value of <value>null</value> means that the size is unknown.</remarks>
		long? BodySize { get; set; }

		/// <summary>
		/// Indicates if a body was returned by the requested target. If false the value of <see cref="BodyStream"/> and
		/// <see cref="BodySize"/> should be ignored.
		/// </summary>
		bool IsBodyAvailable { get; set; }

		/// <summary>
		/// The body as a stream.
		/// </summary>
		Stream BodyStream { get; set; }

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
	}
}
