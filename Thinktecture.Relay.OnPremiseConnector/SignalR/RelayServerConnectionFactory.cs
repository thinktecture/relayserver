using System;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.Heartbeat;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
    internal class RelayServerConnectionFactory : IRelayServerConnectionFactory
    {
        private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
        private readonly ILogger _logger;
        private readonly IHeartbeatMonitor _heartbeatMonitor;

        public RelayServerConnectionFactory(IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger, IHeartbeatMonitor heartbeatMonitor)
        {
            _onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
            _logger = logger;
            _heartbeatMonitor = heartbeatMonitor;
        }

        public IRelayServerConnection Create(string userName, string password, Uri relayServer, int requestTimeout, int maxRetries)
        {
            return new RelayServerConnection(userName, password, relayServer, requestTimeout, maxRetries, _onPremiseTargetConnectorFactory, _logger, _heartbeatMonitor);
        }
    }
}
