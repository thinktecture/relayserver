using System;
using Serilog;
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

		public IRelayServerConnection Create(string userName, string password, Uri relayServer, int requestTimeout)
		{
			return new RelayServerConnection(userName, password, relayServer, requestTimeout, _onPremiseTargetConnectorFactory, _logger);
		}
	}
}
