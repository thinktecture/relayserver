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

		public IRelayServerConnection Create(Assembly versionAssembly, string userName, string password, Uri relayServer, TimeSpan requestTimeout, TimeSpan tokenRefreshWindow, bool logSensitiveData)
		{
			_logger?.Information("Creating new connection for RelayServer {RelayServerUrl} and link user {UserName}", relayServer, userName);
			var httpConnection = new RelayServerHttpConnection(_logger, relayServer, requestTimeout);
			var signalRConnection = new RelayServerSignalRConnection(versionAssembly, userName, password, relayServer, requestTimeout, tokenRefreshWindow, _onPremiseTargetConnectorFactory, httpConnection, _logger, _logSensitiveData);

			// registering connection with maintenance loop
			_maintenanceLoop.RegisterConnection(signalRConnection);
			signalRConnection.Disposing += (o, s) => _maintenanceLoop.UnregisterConnection(o as IRelayServerConnection);

			return signalRConnection;
		}
	}
}
