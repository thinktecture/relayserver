using System.Collections.Generic;
using System.IO;
using System.Net;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	public interface IInterceptedResponse: IOnPremiseTargetResponse
	{
		new HttpStatusCode StatusCode { get; set; }
		new IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		new Stream Stream { get; set; }

		Dictionary<string, string> CloneHttpHeaders();
	}
}
