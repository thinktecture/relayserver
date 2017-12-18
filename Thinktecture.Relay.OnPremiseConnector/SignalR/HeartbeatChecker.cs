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

		public void CheckHeartbeat(IRelayServerConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var logger = _logger?
				.ForContext("RelayServerUri", connection.Uri)
				.ForContext("RelayServerConnectionId", connection.RelayServerConnectionId);

			var intervalWithTolerance = connection.HeartbeatInterval.Add(TimeSpan.FromSeconds(2));
			var lastHeartbeat = connection.LastHeartbeat;

			if (lastHeartbeat != DateTime.MinValue && lastHeartbeat != DateTime.MaxValue)
			{
				logger?.Verbose("Checking last heartbeat time. last-heartbeat={LastHeartbeat}, relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", lastHeartbeat);

				if (lastHeartbeat <= DateTime.UtcNow.Add(-intervalWithTolerance))
				{
					logger?.Warning("Did not receive expected heartbeat; reconnecting. last-heartbeat={LastHeartbeat}, heartbeat-interval={HeartbeatInterval}, relay-server={RelayServerUri}, relay-server-id={RelayServerConnectionId}", lastHeartbeat, connection.HeartbeatInterval);

					connection.Reconnect();
				}
			}
		}
	}
}
