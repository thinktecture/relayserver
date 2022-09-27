using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Thinktecture.Relay.Docker.Authentication;

/// <summary>
/// Class that represents the authentication configuration for api key security.
/// </summary>
public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
	/// <summary>
	/// Gets or sets the header name used for checking for an api key.
	/// </summary>
	public string HeaderName { get; set; } = ApiKeyAuthenticationDefaults.HeaderName;

	/// <summary>
	/// Gets or sets the configured api keys together with the corresponding claims
	/// that will be assumed when the corresponding key is being provided to the api call.
	/// </summary>
	public Dictionary<string, Dictionary<string, string>> ApiKeys { get; set; } = new Dictionary<string, Dictionary<string, string>>();
}
