using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	public interface IRelayTargetResponse
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
		/// The HTTP headers returned by the requested target.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// An array of <see cref="byte"/>s containing the body returned by the requested target.
		/// <remarks>This will be <value>null</value> when the body is too big for inlining.</remarks>
		/// <seealso cref="IsBodyAvailable"/>
		/// </summary>
		byte[] Body { get; set; }

		/// <summary>
		/// The size of an optional body returned by the requested target.
		/// <remarks>A value of <value>null</value> means that the size is unknown.</remarks>
		/// <seealso cref="IsBodyAvailable"/>
		/// </summary>
		long? BodySize { get; set; }

		/// <summary>
		/// Indicates if a body was returned by the requested target. If false the value of <see cref="Body"/> and
		/// <see cref="BodySize"/> should be ignored.
		/// </summary>
		bool IsBodyAvailable { get; set; }

		/// <summary>
		/// The time when the target was requested in behalf.
		/// <remarks>This will only be set when tracing is enabled.</remarks>
		/// </summary>
		DateTime? RequestStart { get; set; }

		/// <summary>
		/// The duration until the target returned its results.
		/// <remarks>This will only be set when tracing is enabled.</remarks>
		/// </summary>
		TimeSpan? RequestDuration { get; set; }
	}
}
