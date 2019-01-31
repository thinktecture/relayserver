using System;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Dto
{
	public class LinkConfiguration
	{
		public TimeSpan? TokenRefreshWindow { get; set; }
		public TimeSpan? HeartbeatInterval { get; set; }

		public TimeSpan? ReconnectMinWaitTime { get; set; }
		public TimeSpan? ReconnectMaxWaitTime { get; set; }

		public TimeSpan? AbsoluteConnectionLifetime { get; set; }
		public TimeSpan? SlidingConnectionLifetime { get; set; }

		public void ApplyDefaults(IConfiguration configuration)
		{
			TokenRefreshWindow = TokenRefreshWindow ?? configuration.LinkTokenRefreshWindow;
			HeartbeatInterval = HeartbeatInterval ?? new TimeSpan(configuration.ActiveConnectionTimeout.Ticks / 4);
			ReconnectMinWaitTime = ReconnectMinWaitTime ?? configuration.LinkReconnectMinWaitTime;
			ReconnectMaxWaitTime = ReconnectMaxWaitTime ?? configuration.LinkReconnectMaxWaitTime;
			AbsoluteConnectionLifetime = AbsoluteConnectionLifetime ?? configuration.LinkAbsoluteConnectionLifetime;
			SlidingConnectionLifetime = SlidingConnectionLifetime ?? configuration.LinkSlidingConnectionLifetime;
		}
	}
}
