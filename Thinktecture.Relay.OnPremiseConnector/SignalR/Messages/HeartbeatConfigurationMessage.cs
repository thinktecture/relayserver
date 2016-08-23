using System;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public class HeartbeatConfigurationMessage : BaseMessage
    {
        public override MessageType Type
        {
            get { return MessageType.HeartbeatConfiguration; }
        }

        public TimeSpan Timeout { get; set; }

        public HeartbeatConfigurationMessage()
        {
            Version = "1";
        }
    }
}