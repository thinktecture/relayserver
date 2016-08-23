using Newtonsoft.Json;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public abstract class BaseMessage : IOnPremiseMessage
    {
        public string Version { get; set; }
        public abstract MessageType Type { get; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}