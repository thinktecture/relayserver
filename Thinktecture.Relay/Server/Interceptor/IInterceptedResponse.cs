using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IInterceptedResponse
	{
		/// <summary>
		/// Gets the internal ID of this request
		/// </summary>
		string RequestId { get; }

		/// <summary>
		/// Gets the id of the relay server this request was sent to
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the HTTP status code received from the local target
		/// </summary>
		HttpStatusCode StatusCode { get; set; }

		/// <summary>
		/// Gets the HTTP headers received from the local target
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		Dictionary<string, string> CloneHttpHeaders();
	}
}
