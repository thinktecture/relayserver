using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	public interface IOnPremiseTargetResponse : IDisposable
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
		/// Gets the response stream containing the data from the local target
		/// </summary>
		Stream Stream { get; }
	}
}
