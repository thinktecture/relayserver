using System.Collections.Generic;
using System.IO;
using System.Net;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	/// <summary>
	/// Represents a response that has been intercepted.
	/// </summary>
	public interface IInterceptedResponse: IOnPremiseTargetResponse
	{
		/// <summary>
		/// Gets or sets the status code of the response.
		/// </summary>
		new HttpStatusCode StatusCode { get; set; }

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
