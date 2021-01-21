using System;
using Thinktecture.Relay.Server.Diagnostics;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// Options for the server.
	/// </summary>
	public class RelayServerOptions
	{
		/// <summary>
		/// The default request expiration.
		/// </summary>
		public static readonly TimeSpan DefaultRequestExpiration = TimeSpan.FromSeconds(10);

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
		public TimeSpan? RequestExpiration { get; set; } = DefaultRequestExpiration;

		/// <summary>
		/// The minimum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMaximumDelay"/>
		public TimeSpan ReconnectMinimumDelay { get; set; } = DiscoveryDocument.DefaultReconnectMinimumDelay;

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted again.
		/// </summary>
		/// <seealso cref="ReconnectMinimumDelay"/>
		public TimeSpan ReconnectMaximumDelay { get; set; } = DiscoveryDocument.DefaultReconnectMaximumDelay;

		/// <summary>
		/// The number of seconds used to timeout the handshake between the server and a connector.
		/// </summary>
		/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
		public TimeSpan HandshakeTimeout { get; set; } = DiscoveryDocument.DefaultHandshakeTimeout;

		/// <summary>
		/// The interval used to send keep alive pings in seconds between the server and a connector.
		/// </summary>
		/// <remarks>The concrete use is an implementation detail of the protocols.</remarks>
		public TimeSpan KeepAliveInterval { get; set; } = DiscoveryDocument.DefaultKeepAliveInterval;

		/// <summary>
		/// The verbosity of the <see cref="IRelayRequestLogger{TRequest,TResponse}"/>.
		/// </summary>
		public RelayRequestLoggerLevel RequestLoggerLevel { get; set; } = RelayRequestLoggerLevel.All;
	}
}
