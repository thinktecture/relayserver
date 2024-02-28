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
	/// Constants for HTTP headers.
	/// </summary>
	public static class HeaderNames
	{
		/// <summary>
		/// Enables tracing of the particular request when present.
		/// </summary>
		/// <remarks>The value of the header is ignored.</remarks>
		public const string EnableTracing = "RelayServer-EnableTracing";
	}
}
