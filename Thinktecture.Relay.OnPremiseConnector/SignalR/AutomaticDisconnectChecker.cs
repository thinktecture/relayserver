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
				if (DateTime.UtcNow > connection.ConnectedSince + connection.AbsoluteConnectionLifetime)
				{
					_logger?.Information("Connection reached maximum absolute lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}, connected-since={ConnectedSince}, absolute-connection-lifetime={AbsoluteConnectionLifetime}", connection.RelayServerConnectionInstanceId, connection.ConnectedSince, connection.AbsoluteConnectionLifetime);

					connection.Disconnect();
					return true;
				}
			}

			if (connection.SlidingConnectionLifetime.HasValue && (connection.LastActivity.HasValue || connection.ConnectedSince.HasValue))
			{
				if (DateTime.UtcNow > (connection.LastActivity ?? connection.ConnectedSince + connection.SlidingConnectionLifetime))
				{
					_logger?.Information("Connection reached maximum sliding lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}, last-activity={LastActivity}, connected-since={ConnectedSince}, sliding-connection-lifetime={SlidingConnectionLifetime}", connection.RelayServerConnectionInstanceId, connection.LastActivity, connection.ConnectedSince, connection.SlidingConnectionLifetime);

					connection.Disconnect();
					return true;
				}
			}

			return false;
		}
	}
}
