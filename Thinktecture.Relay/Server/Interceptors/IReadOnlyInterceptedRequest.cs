using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.Server.Interceptors
{
	public interface IReadOnlyInterceptedRequest
	{
		/// <summary>
		/// Gets the internal ID of this request.
		/// </summary>
		string RequestId { get; }

		/// <summary>
		/// Gets the method of this request. May be GET, PUT, POST, PATCH, DELETE, OPTIONS
		/// </summary>
		string HttpMethod { get; }

		/// <summary>
		/// Gets the url this request is targeted at
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Gets the HTTP headers sent along with this request
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; }

		/// <summary>
		/// Gets this request body
		/// </summary>
		byte[] Body { get; }

		/// <summary>
		/// Gets the id of the relay server this request was sent to
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the Id the On Premise Connector should acknowledge with when it receives this request
		/// </summary>
		string AcknowledgeId { get; }

		/// <summary>
		/// Gets the IP address of the client sending this request. May be an IPv4 or an IPv6 address.
		/// </summary>
		IPAddress ClientIpAddress { get; }
	}
}
