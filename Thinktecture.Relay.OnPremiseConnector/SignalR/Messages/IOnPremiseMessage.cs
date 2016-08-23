namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public interface IOnPremiseMessage
    { 
        string Version { get; set; }
        MessageType Type { get; }

        string ToJson();
    }
}