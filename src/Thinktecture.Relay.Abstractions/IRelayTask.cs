using System;
using System.Collections.Generic;

namespace Thinktecture.Relay.Abstractions
{
	public interface IRelayTask
	{
		/// <summary>
		/// The unique id of the request.
		/// <remarks>This should not be changed afterwards.</remarks>
		/// </summary>
		Guid RequestId { get; set; }

		/// <summary>
		/// The unique id of the server which created the request.
		/// <remarks>This should not be changed afterwards.</remarks>
		/// </summary>
		Guid RequestOriginId { get; set; }

		/// <summary>
		/// The HTTP headers.
		/// </summary>
		IDictionary<string, string[]> HttpHeaders { get; set; }

		/// <summary>
		/// An array of <see cref="byte"/>s containing the body.
		/// <remarks>This will be <value>null</value> when the body is too big for inlining.</remarks>
		/// <seealso cref="IsBodyAvailable"/>
		/// </summary>
		byte[] Body { get; set; }

		/// <summary>
		/// The size of an optional body.
		/// <remarks>A value of <value>null</value> means that the size is unknown.</remarks>
		/// <seealso cref="IsBodyAvailable"/>
		/// </summary>
		long? BodySize { get; set; }

		/// <summary>
		/// Indicates if a body is available. If false the value of <see cref="Body"/> and <see cref="BodySize"/> should be ignored.
		/// </summary>
		bool IsBodyAvailable { get; set; }
	}
}
