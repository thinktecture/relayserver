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
	/// The authentication audience to use.
	/// </summary>
	public const string AuthenticationAudience = "relayserver";

	/// <summary>
	/// The default policy to use.
	/// </summary>
	public const string DefaultAuthenticationPolicy = "relayserver";

	/// <summary>
	/// The default authentication scope to use.
	/// </summary>
	public const string DefaultAuthenticationScope = "relaying";

	/// <summary>
	/// Constants for HTTP headers.
	/// </summary>
	public static class HeaderNames
	{
		/// <summary>
		/// Enables tracing of the particular request when present.
		/// </summary>
		/// <remarks>The value is ignored.</remarks>
		public const string EnableTracing = "X-RelayServer-EnableTracing";
	}
}
