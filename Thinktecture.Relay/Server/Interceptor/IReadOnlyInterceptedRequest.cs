using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IReadOnlyInterceptedRequest
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
		/// Gets the method of this request
		/// </summary>
		string HttpMethod { get; }

		/// <summary>
		/// Gets the url this request is targeted at
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Gets the HTTP headers to send to the local target
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; }

		/// <summary>
		/// Gets the IP address of the requesting client
		/// </summary>
		IPAddress ClientIpAddress { get; }
	}
}
