using System.Collections.Generic;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    public interface IOnPremiseTargetRequest
    {
        string RequestId { get; }
        string HttpMethod { get; }
        string Url { get; }
        IDictionary<string, string> HttpHeaders { get; }
        byte[] Body { get; }
        string OriginId { get; }
        string AcknowledgeId { get; }
    }
}