using System;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
    internal class HeartbeatInformation
    {
        public string LinkId { get; set; }
        public string ConnectionId { get; set; }
        public int ConnectorVersion { get; set; }
        public Func<IOnPremiseTargetRequest, Task> RequestAction { get; set; }
    }
}
