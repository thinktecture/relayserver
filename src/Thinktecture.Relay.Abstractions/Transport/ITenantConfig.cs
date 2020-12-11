using System;

namespace Thinktecture.Relay.Transport
{
	/// <summary>
	/// The tenant configuration which can be sent to a connector.
	/// </summary>
	public interface ITenantConfig
	{
		/// <summary>
		/// The interval used to send keep alive pings between the server and a connector.
		/// </summary>
		public TimeSpan? KeepAliveInterval { get; set; }

		/// <summary>
		/// Enable tracing for all requests of this particular tenant.
		/// </summary>
		public bool? EnableTracing { get; set; }

		/// <summary>
		/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		public TimeSpan? ReconnectMinimumDelay { get; set; }

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		public TimeSpan? ReconnectMaximumDelay { get; set; }
	}
}
