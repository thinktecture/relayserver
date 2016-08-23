using System;
using Thinktecture.Relay.OnPremiseConnector.SignalR;
using Thinktecture.Relay.OnPremiseConnector.SignalR.Messages;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
    public class ConnectionInformation
    {
        public readonly string LinkId;

        public Features Features { get; set; }

        public DateTime LastHeartbeatReceived { get; set; }

        public readonly Action<IOnPremiseConnectorRequest> SendHeartbeatAction;

        public ConnectionInformation(string linkId, Action<IOnPremiseConnectorRequest> sendHeartbeatAction)
        {
            Features = new Features();
            LinkId = linkId;
            SendHeartbeatAction = sendHeartbeatAction;
        }
    } 
}