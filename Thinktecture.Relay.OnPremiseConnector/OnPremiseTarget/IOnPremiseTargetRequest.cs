using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	public interface IOnPremiseTargetRequest
	{
		/// <summary>
		/// Gets the internal ID of this request.
		/// </summary>
		string RequestId { get; }

		/// <summary>
		/// Gets the id of the relay server this request was sent to
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the Id the On Premise Connector should acknowledge with when it receives this request
		/// </summary>
		string AcknowledgeId { get; }

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
		/// Gets the request body if small enough
		/// </summary>
		byte[] Body { get; }

		/// <summary>
		/// Gets the request stream if the body is too large (was requested by a second request)
		/// </summary>
		Stream Stream { get; }
	}
}
