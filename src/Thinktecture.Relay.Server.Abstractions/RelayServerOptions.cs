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
		/// The minimum delay to wait for until a reconnect of a connector should be attempted in seconds.
		/// </summary>
		/// <remarks>The default value is 30 seconds.</remarks>
		public int ReconnectMinDelay { get; set; } = 30;

		/// <summary>
		/// The maximum delay to wait for until a reconnect of a connector should be attempted in seconds.
		/// </summary>
		/// <remarks>The default value is 300 seconds.</remarks>
		public int ReconnectMaxDelay { get; set; } = 300;
	}
}
