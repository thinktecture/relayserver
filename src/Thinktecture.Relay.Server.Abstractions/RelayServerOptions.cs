using System;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// Options for the server.
	/// </summary>
	public class RelayServerOptions
	{
		/// <summary>
		/// Enables the shortcut processing for client requests.
		/// </summary>
		public bool EnableRequestShortcut { get; set; }

		/// <summary>
		/// Enables the shortcut processing for target responses.
		/// </summary>
		public bool EnableResponseShortcut { get; set; }

		/// <summary>
		/// The expiration time of a request until a response must be received.
		/// </summary>
		/// <remarks>The default value is 10 seconds.</remarks>
		public TimeSpan? RequestExpiration { get; set; } = TimeSpan.FromSeconds(10);

		/// <summary>
		/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <remarks>The default value is 30 seconds.</remarks>
		/// <seealso cref="ReconnectMaximumDelay"/>
		public TimeSpan ReconnectMinimumDelay { get; set; } = TimeSpan.FromSeconds(30);

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <remarks>The default value is 5 minutes.</remarks>
		/// <seealso cref="ReconnectMinimumDelay"/>
		public TimeSpan ReconnectMaximumDelay { get; set; } = TimeSpan.FromMinutes(5);

		/// <summary>
		/// The number of seconds used to timeout the handshake between the server and a connector.
		/// </summary>
		/// <remarks>The default value is 15 seconds. The concrete use is an implementation detail of the protocols.</remarks>
		public TimeSpan HandshakeTimeout { get; set; } = TimeSpan.FromSeconds(15);

		/// <summary>
		/// The interval used to send keep alive pings in seconds between the server and a connector.
		/// </summary>
		/// <remarks>The default value is 15 seconds. The concrete use is an implementation detail of the protocols.</remarks>
		public TimeSpan KeepAliveInterval { get; set; } = TimeSpan.FromSeconds(15);
	}
}
