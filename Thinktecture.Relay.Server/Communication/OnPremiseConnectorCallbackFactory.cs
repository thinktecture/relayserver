using NLog;

namespace Thinktecture.Relay.Server.Communication
{
	internal class OnPremiseConnectorCallbackFactory : IOnPremiseConnectorCallbackFactory
	{
	    private readonly ILogger _logger;

	    public OnPremiseConnectorCallbackFactory(ILogger logger)
	    {
	        _logger = logger;
	    }

	    public IOnPremiseConnectorCallback Create(string requestId)
		{
            _logger.Trace("Creating on premise connector callback for request id {0}", requestId);
			return new OnPremiseConnectorCallback(requestId);
		}
	}
}
