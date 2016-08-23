using System;
using System.Text;
using Newtonsoft.Json;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR.Messages
{
    public class MessageContainer
    {
        public MessageType Type { get; set; }
        public string Message { get; set; }

        public static MessageContainer Create(IOnPremiseMessage message)
        {
            return new MessageContainer()
            {
                Type = message.Type,
                Message = message.ToJson()
            };
        }

        public string ToBase64String()
        {
            return Convert.ToBase64String(ToByteArray());
        }

        public byte[] ToByteArray()
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(this));
        }
    }
}