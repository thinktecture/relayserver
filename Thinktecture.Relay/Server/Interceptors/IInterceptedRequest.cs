using System.Collections.Generic;

namespace Thinktecture.Relay.Server.Interceptors
{
	/// <summary>
	/// A request being provided to <see cref="IOnPremiseRequestInterceptor"/>.
	/// </summary>
	public interface IInterceptedRequest : IReadOnlyInterceptedRequest
	{
		/// <summary>
		/// Gets or sets the HTTP headers sent along with this request
		/// </summary>
		new IDictionary<string, string> HttpHeaders { get; }

		/// <summary>
		/// Gets or sets the method of this request. May be GET, PUT, POST, PATCH, DELETE, OPTIONS
		/// </summary>
		new string HttpMethod { get; set; }

		/// <summary>
		/// Gets or sets the url this request is targeted at
		/// </summary>
		new string Url { get; set; }
	}
}
