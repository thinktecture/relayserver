using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Thinktecture.Relay.Server.OnPremise
{
	/// <summary>
	/// This is the pendant for the client interface IOnPremiseTargetResponse and should be kept separate
	/// </summary>
	public interface IOnPremiseConnectorResponse
	{
		/// <summary>
		/// Gets the internal ID of this request
		/// </summary>
		string RequestId { get; }

		/// <summary>
		/// Gets the id of the RelayServer this request was sent to
		/// </summary>
		Guid OriginId { get; }

		/// <summary>
		/// Gets the start time of the local web request
		/// </summary>
		DateTime RequestStarted { get; }

		/// <summary>
		/// Gets the end time of the local web request
		/// </summary>
		DateTime RequestFinished { get; }

		/// <summary>
		/// Gets the HTTP status code received from the local target
		/// </summary>
		HttpStatusCode StatusCode { get; }

		/// <summary>
		/// Gets the HTTP headers received from the local target
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; }

		/// <summary>
		/// Gets the request body if small enough
		/// </summary>
		byte[] Body { get; }

		/// <summary>
		/// Gets the response stream containing the data from the local target
		/// </summary>
		Stream Stream { get; }

		/// <summary>
		/// Gets the response body size (shouldn't 2GB be enough *cough*)
		/// </summary>
		long ContentLength { get; }
	}
}
