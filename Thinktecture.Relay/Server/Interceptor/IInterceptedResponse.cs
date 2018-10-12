using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.Server.Interceptor
{
	/// <summary>
	/// Represents a response from the on premise connector before it will be relayed to the requesting client.
	/// </summary>
	public interface IInterceptedResponse
	{
		/// <summary>
		/// Gets the internal ID of this request.
		/// </summary>
		string RequestId { get; }

		/// <summary>
		/// Gets the id of the RelayServer this request was sent to.
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the HTTP status code received from the local target.
		/// </summary>
		HttpStatusCode StatusCode { get; set; }

		/// <summary>
		/// Gets the HTTP headers received from the local target.
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		/// <summary>
		/// Creates a copy of the <see cref="HttpHeaders"/> read-only dictionary.
		/// </summary>
		/// <returns>A changeable dictionary containing all HttpHeaders of the intercepted response.</returns>
		Dictionary<string, string> CloneHttpHeaders();
	}
}
