using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class HeartbeatChecker: IHeartbeatChecker
	{
		private readonly ILogger _logger;

		public HeartbeatChecker(ILogger logger)
		{
			_logger = logger;
		}

		public void Check(IRelayServerConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var logger = _logger?
				.ForContext("RelayServerUri", connection.Uri)
				.ForContext("RelayServerConnectionInstanceId", connection.RelayServerConnectionInstanceId);

			var lastHeartbeat = connection.LastHeartbeat;
			if (connection.HeartbeatSupportedByServer && lastHeartbeat != DateTime.MinValue)
			{
				if (lastHeartbeat <= DateTime.UtcNow.Subtract(connection.HeartbeatInterval.Add(TimeSpan.FromSeconds(2))))
				{
					logger?.Warning("Did not receive expected heartbeat. last-heartbeat={LastHeartbeat}, heartbeat-interval={HeartbeatInterval}, relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}", lastHeartbeat, connection.HeartbeatInterval);

					connection.Reconnect();
				}
			}
		}
	}
}
