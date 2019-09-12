using System.Collections.Generic;
using System.IO;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	/// <summary>
	/// Represents a request that has been intercepted.
	/// </summary>
	public interface IInterceptedRequest : IOnPremiseTargetRequest
	{
		/// <summary>
		/// Gets or sets the Http method of the request.
		/// </summary>
		new string HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the Url of the request (Path with query)
		/// </summary>
		new string Url { get; set; }

		/// <summary>
		/// Gets or sets the Http headers of the request.
		/// </summary>
		new IReadOnlyDictionary<string, string> HttpHeaders { get; set; }

		/// <summary>
		/// Gets or sets the content stream of the request.
		/// </summary>
		new Stream Stream { get; set; }

		/// <summary>
		/// Creates a copy of the http headers dictionary.
		/// </summary>
		/// <returns>A new dictionary with the http headers.</returns>
		Dictionary<string, string> CloneHttpHeaders();
	}
}
