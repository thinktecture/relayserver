using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class AutomaticDisconnectChecker: IAutomaticDisconnectChecker
	{
		private readonly ILogger _logger;

		public AutomaticDisconnectChecker(ILogger logger)
		{
			_logger = logger;
		}

		public bool DisconnectIfRequired(IRelayServerConnection connection)
		{
			if (connection.AbsoluteConnectionLifetime.HasValue && connection.ConnectedSince.HasValue)
			{
				_logger?.Debug("Checking if connection reached maximum absolute lifetime. relay-server-connection-instance-id={RelayServerConnectionInstanceId}", connection.RelayServerConnectionInstanceId);

				var endOfMaximumConnectionTime = connection.ConnectedSince + connection.AbsoluteConnectionLifetime;

				if (DateTime.UtcNow > endOfMaximumConnectionTime)
				{
					_logger?.Information("Connection reached maximum absolute lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}", connection.RelayServerConnectionInstanceId);
					connection.Disconnect();
					return true;
				}
			}

			if (connection.SlidingConnectionLifetime.HasValue && connection.LastActivity.HasValue)
			{
				_logger?.Debug("Checking if connection reached maximum sliding lifetime. relay-server-connection-instance-id={RelayServerConnectionInstanceId}", connection.RelayServerConnectionInstanceId);

				var endOfSlidingConnectionTime = connection.LastActivity + connection.SlidingConnectionLifetime;

				if (DateTime.UtcNow > endOfSlidingConnectionTime)
				{
					_logger?.Information("Connection reached maximum sliding lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}", connection.RelayServerConnectionInstanceId);

					connection.Disconnect();
					return true;
				}
			}

			return false;
		}
	}
}
