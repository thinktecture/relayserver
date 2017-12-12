using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IInterceptedRequest : IReadOnlyInterceptedRequest
	{
		/// <summary>
		/// Gets the method of this request
		/// <remarks>This can be set by an interceptor</remarks>
		/// </summary>
		new string HttpMethod { get; set; }

		/// <summary>
		/// Gets the url this request is targeted at
		/// <remarks>This can be set by an interceptor</remarks>
		/// </summary>
		new string Url { get; set; }

		/// <summary>
		/// Gets the HTTP headers to send to the local target
		/// <remarks>This can be set by an interceptor</remarks>
		/// </summary>
		new IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		Dictionary<string, string> CloneHttpHeaders();

		/// <summary>
		/// Determines, whether this request will always be send to an on premise connector
		/// even when an interceptor directly answers this request
		/// </summary>
		bool AlwaysSendToOnPremiseConnector { get; set; }

		/// <summary>
		/// Gets whether this request should be auto-acknowledged when taken from the RabbitMQ
		/// <remarks>This can be set by an interceptor</remarks>
		/// </summary>
		bool AutoAcknowledge { get; set; }
	}
}
