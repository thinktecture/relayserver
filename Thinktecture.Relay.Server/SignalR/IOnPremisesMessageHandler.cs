using Microsoft.AspNet.SignalR;

namespace Thinktecture.Relay.Server.SignalR
{
    public interface IOnPremisesMessageHandler
    {
        void Received(IConnection connection, IRequest request, string connectionId, string data);
    }
}