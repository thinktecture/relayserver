using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
	public interface IOnPremiseTargetResponse
	{
		string RequestId { get; }
		Guid OriginId { get; }
		DateTime RequestStarted { get; }
		DateTime RequestFinished { get; }
		IReadOnlyDictionary<string, string> HttpHeaders { get; }
		HttpStatusCode StatusCode { get; }
		byte[] Body { get; }
	}
}
