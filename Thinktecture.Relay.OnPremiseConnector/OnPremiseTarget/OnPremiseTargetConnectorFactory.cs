using System;
using NLog;

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
            return new OnPremiseWebTargetConnector(baseUri, requestTimeout, _logger);
        }

        public IOnPremiseTargetConnector Create(Type handlerType, int requestTimeout)
        {
            return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerType);
        }

	    public IOnPremiseTargetConnector Create(Func<IOnPremiseInProcHandler> handlerFactory, int requestTimeout)
	    {
			return new OnPremiseInProcTargetConnector(_logger, requestTimeout, handlerFactory);
		}

		public IOnPremiseTargetConnector Create<T>(int requestTimeout)
			where T: IOnPremiseInProcHandler, new()
        {
            return new OnPremiseInProcTargetConnector<T>(requestTimeout, _logger);
        }
    }
}
