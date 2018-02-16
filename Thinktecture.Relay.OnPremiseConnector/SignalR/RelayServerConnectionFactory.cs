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

		public IRelayServerConnection Create(Assembly versionAssembly, string userName, string password, Uri relayServer, int requestTimeout, int tokenRefreshWindow)
		{
			_logger?.Information("Creating new connection for RelayServer {RelayServerUrl} and link user {UserName}", relayServer, userName);
			var connection = new RelayServerConnection(versionAssembly, userName, password, relayServer, requestTimeout, tokenRefreshWindow, _onPremiseTargetConnectorFactory, _logger);

			// registering connection with maintenance loop
			_maintenanceLoop.RegisterConnection(connection);
			connection.Disposing += (o, s) => _maintenanceLoop.UnregisterConnection(o as IRelayServerConnection);

			return connection;
		}
	}
}
