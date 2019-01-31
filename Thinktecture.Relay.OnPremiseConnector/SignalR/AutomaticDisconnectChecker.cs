using System;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class AutomaticDisconnectChecker : IAutomaticDisconnectChecker
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
					_logger?.Information("Disconnecting because connection instance {RelayServerConnectionInstanceId} reached its maximum absolute lifetime of {AbsoluteConnectionLifetime} since {ConnectedSince}", connection.RelayServerConnectionInstanceId, connection.AbsoluteConnectionLifetime, connection.ConnectedSince);

					connection.Disconnect();
					return true;
				}
			}

			if (connection.SlidingConnectionLifetime.HasValue && (connection.LastActivity.HasValue || connection.ConnectedSince.HasValue))
			{
				if (DateTime.UtcNow > (connection.LastActivity ?? connection.ConnectedSince + connection.SlidingConnectionLifetime))
				{
					_logger?.Information("Disconnecting because connection instance {RelayServerConnectionInstanceId} reached its maximum sliding lifetime of {SlidingConnectionLifetime} since {LastActivity}", connection.RelayServerConnectionInstanceId, connection.SlidingConnectionLifetime, connection.LastActivity ?? connection.ConnectedSince);

					connection.Disconnect();
					return true;
				}
			}

			return false;
		}
	}
}
