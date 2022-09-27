using System;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Thinktecture.Relay.Docker.Authentication;

/// <summary>
/// Extension methods to configure api key authentication.
/// </summary>
public static class ApiKeyExtensions
{
	/// <summary>
	/// Adds api key authentication to <see cref="AuthenticationBuilder"/> using the specified scheme.
	/// <para>
	/// Api key authentication uses a HTTP header to perform authentication.
	/// </para>
	/// </summary>
	/// <param name="builder">The <see cref="AuthenticationBuilder"/>.</param>
	/// <param name="authenticationScheme">The authentication scheme.</param>
	/// <param name="displayName">A display name for the authentication handler.</param>
	/// <param name="configureOptions">A delegate to configure <see cref="ApiKeyAuthenticationOptions"/>.</param>
	/// <returns>A reference to <paramref name="builder"/> after the operation has completed.</returns>
	public static AuthenticationBuilder AddApiKey(this AuthenticationBuilder builder, string authenticationScheme, string? displayName, Action<ApiKeyAuthenticationOptions> configureOptions)
	{
		builder.Services.AddOptions<ApiKeyAuthenticationOptions>(authenticationScheme);
		return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(authenticationScheme, displayName, configureOptions);
	}
}
