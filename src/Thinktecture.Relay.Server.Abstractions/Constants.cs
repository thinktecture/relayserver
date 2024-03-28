namespace Thinktecture.Relay.Server;

/// <summary>
/// Constants for use with RelayServer.
/// </summary>
public static class Constants
{
	/// <summary>
	/// The authentication scheme to use.
	/// </summary>
	public const string DefaultAuthenticationScheme = "relayserver";

	/// <summary>
	/// The default authentication audience to use.
	/// </summary>
	public const string DefaultAuthenticationAudience = "relayserver";

	/// <summary>
	/// The default policy to use.
	/// </summary>
	public const string DefaultAuthenticationPolicy = "relayserver";

	/// <summary>
	/// The default authentication scope to use.
	/// </summary>
	public const string DefaultAuthenticationScope = "connector";

	/// <summary>
	/// The default relay path to use.
	/// </summary>
	public const string DefaultRelayPath = "relay";

	/// <summary>
	/// The default queue path to use.
	/// </summary>
	public const string DefaultQueuePath = "queue";

	/// <summary>
	/// The default trace path to use.
	/// </summary>
	public const string DefaultTracePath = "trace";

	/// <summary>
	/// Constants for HTTP headers.
	/// </summary>
	public static class HeaderNames
	{
		/// <summary>
		/// Contains the unique id of the request.
		/// </summary>
		/// <remarks>This will only be present when tracing is enabled.</remarks>
		public const string RequestId = "RelayServer-RequestId";

		/// <summary>
		/// Contains the machine name of the server handling the request.
		/// </summary>
		/// <remarks>This will only be present when tracing is enabled.</remarks>
		public const string ServerMachineName = "RelayServer-Server-MachineName";

		/// <summary>
		/// Contains the version of the server handling the request.
		/// </summary>
		/// <remarks>This will only be present when tracing is enabled.</remarks>
		public const string ServerVersion = "RelayServer-Server-Version";

		/// <summary>
		/// Contains the start timestamp of the target handling the request.
		/// </summary>
		/// <remarks>This will only be present when tracing is enabled.</remarks>
		public const string TargetStart = "RelayServer-TargetStart";

		/// <summary>
		/// Contains the duration of the target handling the request.
		/// </summary>
		/// <remarks>This will only be present when tracing is enabled.</remarks>
		public const string TargetDuration = "RelayServer-TargetDuration";
	}
}
