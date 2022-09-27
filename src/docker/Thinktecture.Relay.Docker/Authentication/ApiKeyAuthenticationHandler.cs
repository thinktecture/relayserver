using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Thinktecture.Relay.Docker.Authentication;

/// <summary>
/// Example handler that allows "securing" an api with configurable api keys.
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
	/// <inheritdoc cref="AuthenticationHandler<ApiKeyAuthenticationOptions>"/>
	public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
		ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock)
		: base(options, logger, encoder, clock)
	{
	}

	protected override Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (Context.Request.Headers.TryGetValue(Options.HeaderName, out var apiKeys) &&
		    !String.IsNullOrEmpty(apiKeys.First()) &&
		    Options.ApiKeys.TryGetValue(apiKeys.First(), out var claims))
		{
			return Task.FromResult(AuthenticateResult.Success(BuildTicket(claims)));
		}

		return Task.FromResult(AuthenticateResult.NoResult());
	}

	private AuthenticationTicket BuildTicket(Dictionary<string, string> claims)
		=> new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(
			claims.Select(c => new Claim(c.Key, c.Value)))), Scheme.Name);
}
