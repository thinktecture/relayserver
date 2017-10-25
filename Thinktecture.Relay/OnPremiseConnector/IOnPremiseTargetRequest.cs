using System;
using System.Collections.Generic;
using System.Net;
using System.IO;

namespace Thinktecture.Relay.OnPremiseConnector
{
	public interface IOnPremiseTargetRequest
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
		/// Gets this request stream
		/// </summary>
		Stream Stream { get; }

		/// <summary>
		/// Gets the id of the relay server this request was sent to
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the Id the On Premise Connector should acknowledge with when it receives this request
		/// </summary>
		string AcknowledgeId { get; }

		/// <summary>
		/// Gets the IP Address of the client
		/// </summary>
		IPAddress ClientIpAddress { get; }
	}
}
