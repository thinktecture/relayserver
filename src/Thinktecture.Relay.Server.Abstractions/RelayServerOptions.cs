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
	}
}
