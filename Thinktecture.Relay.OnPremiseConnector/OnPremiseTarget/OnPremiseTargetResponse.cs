using System;
using System.Collections.Generic;
using System.Net;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    internal class OnPremiseTargetResponse : IOnPremiseTargetResponse
    {
        public string RequestId { get; set; }
        public string OriginId { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public IDictionary<string, string> HttpHeaders { get; set; }
        public byte[] Body { get; set; }

        public DateTime RequestStarted { get; set; }
        public DateTime RequestFinished { get; set; }
    }
}
