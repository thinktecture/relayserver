using System;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
    public class RegistrationInformation
    {
        public string ConnectionId { get; set; }
        public string OnPremiseId { get; set; }
        public Action<IOnPremiseConnectorRequest> RequestAction { get; set; }
        public Action<IOnPremiseConnectorRequest> SendHeartbeatAction { get; set; }
        public string IpAddress { get; set; }
    }
}