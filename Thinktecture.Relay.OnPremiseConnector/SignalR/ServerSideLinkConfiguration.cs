using System;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class ServerSideLinkConfiguration
	{
		public TimeSpan? TokenRefreshWindow { get; set; }
		public TimeSpan? HeartbeatInterval { get; set; }

		public TimeSpan? ReconnectMinWaitTime { get; set; }
		public TimeSpan? ReconnectMaxWaitTime { get; set; }

		public TimeSpan? AbsoluteConnectionLifetime { get; set; }
		public TimeSpan? SlidingConnectionLifetime { get; set; }
	}
}
