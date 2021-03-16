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

			connection.CheckHeartbeat();
		}
	}
}
