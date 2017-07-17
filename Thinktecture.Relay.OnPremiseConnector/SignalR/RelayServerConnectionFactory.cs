using System;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
    internal class RelayServerConnectionFactory : IRelayServerConnectionFactory
    {
        private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;
        private readonly ILogger _logger;

        public RelayServerConnectionFactory(IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory, ILogger logger)
        {
            _onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory;
            _logger = logger;
        }

        public IRelayServerConnection Create(string userName, string password, Uri relayServer, int requestTimeout, int maxRetries)
        {
            return new RelayServerConnection(userName, password, relayServer, requestTimeout, maxRetries, _onPremiseTargetConnectorFactory, _logger);
        }
    }
}
