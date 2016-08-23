namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public class FeaturesMessage : BaseMessage
    {
        public FeaturesMessage()
        {
            Features = new Features();
            Version = "1";
        }

        public Features Features { get; set; }

        public override MessageType Type
        {
            get { return MessageType.Feature; }
        }
    }
}