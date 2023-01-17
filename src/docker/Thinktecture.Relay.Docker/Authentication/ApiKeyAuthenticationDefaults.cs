namespace Thinktecture.Relay.Docker.Authentication;

/// <summary>
/// Default values related to api key - based authentication handler
/// </summary>
public static class ApiKeyAuthenticationDefaults
{
	/// <summary>
	/// The default value used for ApiKeyAuthenticationOptions.AuthenticationScheme.
	/// </summary>
	public const string AuthenticationScheme = "ApiKey";

	/// <summary>
	/// The default header name used for the api key authentication.
	/// </summary>
	public const string HeaderName = "Api-Key";
}
