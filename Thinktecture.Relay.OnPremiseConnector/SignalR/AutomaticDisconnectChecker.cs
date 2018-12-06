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
				var endOfMaximumConnectionTime = connection.ConnectedSince + connection.AbsoluteConnectionLifetime;

				if (DateTime.UtcNow > endOfMaximumConnectionTime)
				{
					_logger?.Information("Connection reached maximum absolute lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}, connected-since={ConnectedSince}, absolute-connection-lifetime={AbsoluteConnectionLifetime}", connection.RelayServerConnectionInstanceId, connection.ConnectedSince, connection.AbsoluteConnectionLifetime);
					connection.Disconnect();
					return true;
				}
			}

			if (connection.SlidingConnectionLifetime.HasValue && connection.LastActivity.HasValue)
			{
				var endOfSlidingConnectionTime = connection.LastActivity + connection.SlidingConnectionLifetime;

				if (DateTime.UtcNow > endOfSlidingConnectionTime)
				{
					_logger?.Information("Connection reached maximum sliding lifetime: Disconnecting. relay-server-connection-instance-id={RelayServerConnectionInstanceId}, last-activity={LastActivity}, sliding-connection-lifetime={SlidingConnectionLifetime}", connection.RelayServerConnectionInstanceId, connection.LastActivity, connection.SlidingConnectionLifetime);

					connection.Disconnect();
					return true;
				}
			}

			return false;
		}
	}
}
