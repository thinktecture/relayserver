using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    public interface IOnPremiseTargetResponse
    {
        string RequestId { get; }
        string OriginId { get; }
        HttpStatusCode StatusCode { get; set; }
        IDictionary<string, string> HttpHeaders { get; }
        byte[] Body { get; set; }
        DateTime RequestStarted { get; }
        DateTime RequestFinished { get; }
    }
}