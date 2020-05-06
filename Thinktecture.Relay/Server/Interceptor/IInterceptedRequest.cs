using System;
using System.Collections.Generic;
using System.IO;

namespace Thinktecture.Relay.Server.Interceptor
{
	/// <summary>
	/// Represents a clients request that can be processed by an interceptor before relaying it to the on premise connector.
	/// </summary>
	public interface IInterceptedRequest : IReadOnlyInterceptedRequest
	{
		/// <summary>
		/// Gets the method of this request.
		/// <remarks>This can be set by an interceptor.</remarks>
		/// </summary>
		new string HttpMethod { get; set; }

		/// <summary>
		/// Gets the url this request is targeted at.
		/// <remarks>This can be set by an interceptor.</remarks>
		/// </summary>
		new string Url { get; set; }

		/// <summary>
		/// Gets the HTTP headers to send to the local target.
		/// <remarks>This can be set by an interceptor.</remarks>
		/// </summary>
		new IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		/// <summary>
		/// Determines whether this request will always be send to an on-premise connector.
		/// even when an interceptor directly answers this request.
		/// </summary>
		bool AlwaysSendToOnPremiseConnector { get; set; }

		/// <summary>
		/// Gets the request TTL within the RabbitMQ.
		/// <remarks>This can be set by an interceptor.</remarks>
		/// </summary>
		TimeSpan Expiration { get; set; }

		/// <summary>
		/// Gets the acknowledgment mode that determines how and when the request should be acknowledged in the message queue.
		/// <remarks>This can be set by an interceptor.</remarks>
		/// </summary>
		AcknowledgmentMode AcknowledgmentMode { get; set; }

		/// <summary>
		/// Gets or sets the content of the request.
		/// <remarks>Accessing this property will COPY the original content and use more memory.</remarks>
		/// </summary>
		Stream Content { get; set; }

		/// <summary>
		/// Creates a copy of the <see cref="HttpHeaders"/> read-only dictionary.
		/// <remarks>This can use this to create a copy, modify it, and set it in an interceptor.</remarks>
		/// </summary>
		/// <returns>A changeable dictionary containing all HttpHeaders of the intercepted request.</returns>
		Dictionary<string, string> CloneHttpHeaders();

		/// <summary>
		/// Gets the additional properties which will be serialized onto the root object
		/// </summary>
		IReadOnlyDictionary<string, object> Properties { get; set; }
	}
}
