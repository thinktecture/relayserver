using Serilog;

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
			_logger?.Verbose("Creating on premise connector callback. request-id={RequestId}", requestId);

			return new OnPremiseConnectorCallback(requestId);
		}
	}
}
