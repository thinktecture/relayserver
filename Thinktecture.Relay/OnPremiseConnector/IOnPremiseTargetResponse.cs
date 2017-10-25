using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector
{
	public interface IOnPremiseTargetResponse
	{
		string RequestId { get; }
		Guid OriginId { get; }
		DateTime RequestStarted { get; }
		DateTime RequestFinished { get; }
		IDictionary<string, string> HttpHeaders { get; }
		HttpStatusCode StatusCode { get; set; }
		Stream Stream { get; set; }

		[Obsolete("This is a legacy property for v1 OnPremiseConnector")]
		byte[] Body { get; }
	}
}
