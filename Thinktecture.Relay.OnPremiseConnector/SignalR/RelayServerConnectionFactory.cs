using System;
using System.Reflection;
using Serilog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class RelayServerConnectionFactory : IRelayServerConnectionFactory
	{
		private readonly ILogger _logger;
		private readonly IMaintenanceLoop _maintenanceLoop;
		private readonly IOnPremiseTargetConnectorFactory _onPremiseTargetConnectorFactory;

		public RelayServerConnectionFactory(ILogger logger, IMaintenanceLoop maintenanceLoop, IOnPremiseTargetConnectorFactory onPremiseTargetConnectorFactory)
		{
			_logger = logger;
			_maintenanceLoop = maintenanceLoop ?? throw new ArgumentNullException(nameof(maintenanceLoop));
			_onPremiseTargetConnectorFactory = onPremiseTargetConnectorFactory ?? throw new ArgumentNullException(nameof(onPremiseTargetConnectorFactory));
		}

		public IRelayServerConnection Create(Assembly entryAssembly, string userName, string password, Uri relayServer, int requestTimeoutInSeconds, int tokenRefreshWindowInSeconds)
		{
			_logger?.Information("Creating new connection for relay server {RelayServerUrl} and link user {UserName}", relayServer, userName);
			var connection = new RelayServerConnection(entryAssembly, userName, password, relayServer, requestTimeoutInSeconds, tokenRefreshWindowInSeconds, _onPremiseTargetConnectorFactory, _logger);

			// registering connection with maintenance loop
			_maintenanceLoop.RegisterConnection(connection);
			connection.Disposing += (o, s) => _maintenanceLoop.UnregisterConnection(o as IRelayServerConnection);

			return connection;
		}
	}
}
