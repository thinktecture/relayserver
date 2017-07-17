using System;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.OnPremise
{
    public interface IOnPremiseConnectorRequest : IOnPremiseTargetRequest
    {
        DateTime RequestStarted { get; }
        DateTime RequestFinished { get; }
    }
}