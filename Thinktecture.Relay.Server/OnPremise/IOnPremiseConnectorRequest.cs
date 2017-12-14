using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.Server.OnPremise
{
	/// <summary>
	/// This is the pendant for the client interface IOnPremiseTargetRequest and should be kept separate
	/// </summary>
	public interface IOnPremiseConnectorRequest
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
		/// Gets the Id the On Premise Connector should acknowledge with when it receives this request
		/// </summary>
		string AcknowledgeId { get; }

		/// <summary>
		/// Gets the start time of the incoming request
		/// </summary>
		DateTime RequestStarted { get; }

		/// <summary>
		/// Gets the end time of the incoming request
		/// <remarks>This will be set when request is finished</remarks>
		/// </summary>
		DateTime RequestFinished { get; set; }

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
		/// Gets whether this request should be auto-acknowledged when taken from the RabbitMQ
		/// </summary>
		AcknowledgmentMode AcknowledgmentMode { get; }

		/// <summary>
		/// Gets the request stream if the body is too large (was requested by a second request)
		/// </summary>
		Stream Stream { get; }

		/// <summary>
		/// Gets the request body size (shouldn't 2GB be enough *cough*)
		/// </summary>
		long ContentLength { get; }

		/// <summary>
		/// Determines, whether this request will always be send to an on premise connector
		/// even when an interceptor directly answers this request
		/// </summary>
		bool AlwaysSendToOnPremiseConnector { get; }

		/// <summary>
		/// Gets the request TTL within the RabbitMQ
		/// </summary>
		TimeSpan Expiration { get; }
	}
}
