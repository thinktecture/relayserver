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
	}
}
