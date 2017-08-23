using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    public interface IOnPremiseTargetResponse
    {
        string RequestId { get; }
		Guid OriginId { get; }
        HttpStatusCode StatusCode { get; }
        IReadOnlyDictionary<string, string> HttpHeaders { get; }
        byte[] Body { get; }
        DateTime RequestStarted { get; }
        DateTime RequestFinished { get; }
    }
}
