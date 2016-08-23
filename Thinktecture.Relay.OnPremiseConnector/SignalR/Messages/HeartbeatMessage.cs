namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public class HeartbeatMessage : BaseMessage
    {
        public override MessageType Type
        {
            get { return MessageType.Heartbeat; }
        }
    }
}