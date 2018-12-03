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
					connection.Disconnect();
					return true;
				}
			}

			if (connection.SlidingConnectionLifetime.HasValue && connection.LastActivity.HasValue)
			{
				var endOfSlidingConnectionTime = connection.LastActivity + connection.SlidingConnectionLifetime;

				if (DateTime.UtcNow > endOfSlidingConnectionTime)
				{
					connection.Disconnect();
					return true;
				}
			}

			return false;
		}
	}
}
