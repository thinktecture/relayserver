using System;
using NLog.Interface;

namespace Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget
{
    internal class OnPremiseTargetConnectorFactory : IOnPremiseTargetConnectorFactory
    {
        private readonly ILogger _logger;

        public OnPremiseTargetConnectorFactory(ILogger logger)
        {
            _logger = logger;
        }

        public IOnPremiseTargetConnector Create(Uri baseUri, int requestTimeout)
        {
            return new OnPremiseTargetConnector(baseUri, requestTimeout, _logger);
        }
    }
}
