using System.Collections.Generic;
using System.IO;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	public interface IInterceptedRequest : IOnPremiseTargetRequest
	{
		new string HttpMethod { get; set; }
		new string Url { get; set; }
		new IReadOnlyDictionary<string, string> HttpHeaders { get; set; }
		new Stream Stream { get; set; }

		Dictionary<string, string> CloneHttpHeaders();
	}
}
