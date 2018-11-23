using System;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkConfiguration
	{
		public TimeSpan? TokenRefreshWindow { get; set; }
		public TimeSpan? HeartbeatInterval { get; set; }

		public TimeSpan? RelayRequestTimeout { get; set; }

		public TimeSpan? ReconnectMinWaitTime { get; set; }
		public TimeSpan? ReconnectMaxWaitTime { get; set; }

		public TimeSpan? AbsoluteConnectionLifetime { get; set; }
		public TimeSpan? SlidingConnectionLifetime { get; set; }

		public void ApplyDefaults(IConfiguration configuration)
		{
			TokenRefreshWindow = TokenRefreshWindow ?? configuration.LinkTokenRefreshWindowDefault;
			HeartbeatInterval = HeartbeatInterval ?? new TimeSpan(configuration.ActiveConnectionTimeout.Ticks / 4);
			RelayRequestTimeout = RelayRequestTimeout ?? configuration.LinkRelayRequestTimeoutDefault;
			ReconnectMinWaitTime = ReconnectMinWaitTime ?? configuration.LinkReconnectMinWaitTimeDefault;
			ReconnectMaxWaitTime = ReconnectMaxWaitTime ?? configuration.LinkReconnectMaxWaitTimeDefault;
			AbsoluteConnectionLifetime = AbsoluteConnectionLifetime ?? configuration.LinkAbsoluteConnectionLifetimeDefault;
			SlidingConnectionLifetime = SlidingConnectionLifetime ?? configuration.LinkSlidingConnectionLifetimeDefault;
		}
	}
}
