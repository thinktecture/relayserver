using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	internal interface IOnPremiseTargetRequestInternal : IOnPremiseTargetRequest
	{
		/// <summary>
		/// Gets the request body if small enough
		/// </summary>
		byte[] Body { get; }

		/// <summary>
		/// Gets the request stream if the body is too large (was requested by a second request)
		/// </summary>
		new Stream Stream { get; set; }

		bool IsPingRequest { get; }
		bool IsHeartbeatRequest { get; }
		bool IsHeartbeatOrPingRequest { get; }
	}

	/// <summary>
	/// This is the pendant for the server interface IOnPremiseConnectorRequest and should be kept separate
	/// </summary>
	public interface IOnPremiseTargetRequest
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
		/// Gets the id the on-premise connector should acknowledge with when it receives this request
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
		/// Gets the request stream if the body is too large (was requested by a second request)
		/// </summary>
		Stream Stream { get; }

		/// <summary>
		/// Gets the mode of acknowledgment of this request
		/// </summary>
		AcknowledgmentMode AcknowledgmentMode { get; }

		/// <summary>
		/// Gets the id of the RelayServer this request may acknowledged to
		/// </summary>
		Guid AcknowledgeOriginId { get; }
	}
}
