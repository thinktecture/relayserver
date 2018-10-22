using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Principal;

namespace Thinktecture.Relay.Server.Interceptor
{
	/// <summary>
	/// Represents a request that has been intercepted.
	/// </summary>
	public interface IReadOnlyInterceptedRequest
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
		/// Gets the method of this request.
		/// </summary>
		string HttpMethod { get; }

		/// <summary>
		/// Gets the url this request is targeted at.
		/// </summary>
		string Url { get; }

		/// <summary>
		/// Gets the HTTP headers to send to the local target.
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; }

		/// <summary>
		/// Gets the IP address of the requesting client.
		/// </summary>
		IPAddress ClientIpAddress { get; }

		/// <summary>
		/// Gets the client user.
		/// </summary>
		IPrincipal ClientUser { get; }

		/// <summary>
		/// Gets the Uri of this request
		/// </summary>
		Uri Uri { get; }
	}
}
