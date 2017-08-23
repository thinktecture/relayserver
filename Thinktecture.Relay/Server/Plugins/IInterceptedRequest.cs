using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// A request being provided to <see cref="IOnPremiseRequestInterceptor"/>.
	/// </summary>
	public interface IInterceptedRequest : IOnPremiseConnectorRequest
	{
		new byte[] Body { get; set; }
		new IDictionary<string, string> HttpHeaders { get; set; }
		new string HttpMethod { get; set; }
		new string Url { get; set; }
	}
}
