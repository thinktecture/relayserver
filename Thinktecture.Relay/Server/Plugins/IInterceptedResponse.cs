using System;
using System.Collections.Generic;
using System.Net;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Plugins
{
	/// <summary>
	/// A response being provided to <see cref="IOnPremiseResponseInterceptor"/>.
	/// </summary>
	public interface IInterceptedResponse : IOnPremiseTargetResponse
	{
		new IDictionary<string, string> HttpHeaders { get; set; }
		new HttpStatusCode StatusCode { get; set; }
		new byte[] Body { get; set; }
	}
}
