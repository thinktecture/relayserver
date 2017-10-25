using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// A request being provided to <see cref="IOnPremiseRequestInterceptor"/>.
	/// </summary>
	public interface IInterceptedRequest : IReadOnlyInterceptedRequest
	{
		/// <summary>
		/// Gets or sets this request body
		/// </summary>
		new byte[] Body { get; set; }

		/// <summary>
		/// Gets or sets the HTTP headers sent along with this request
		/// </summary>
		new Dictionary<string, string> HttpHeaders { get; set; }

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
