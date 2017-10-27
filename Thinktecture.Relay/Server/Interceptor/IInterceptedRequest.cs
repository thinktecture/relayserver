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
	}
}
