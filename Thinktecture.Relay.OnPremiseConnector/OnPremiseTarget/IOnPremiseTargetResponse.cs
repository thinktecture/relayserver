using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	/// <summary>
	/// This is the pendant for the server interface IOnPremiseConnectorResponse and should be kept separate
	/// </summary>
	public interface IOnPremiseTargetResponse : IDisposable
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
		/// <remarks>This can be set by an in-proc handler</remarks>
		/// </summary>
		HttpStatusCode StatusCode { get; set; }

		/// <summary>
		/// Gets the HTTP headers received from the local target
		/// <remarks>This can be set by an in-proc handler</remarks>
		/// </summary>
		IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		/// <summary>
		/// Gets the response stream containing the data from the local target
		/// <remarks>This can be set by an in-proc handler</remarks>
		/// </summary>
		Stream Stream { get; set; }
	}
}
