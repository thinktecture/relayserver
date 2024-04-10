using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Thinktecture.Relay.Server.Extensions;

/// <summary>
/// A struct containing the parsed request url.
/// </summary>
/// <param name="Mode">The requested mode.</param>
/// <param name="TenantName">The unique name of the tenant.</param>
/// <param name="Target">The name of the target.</param>
/// <param name="Url">The url for the target.</param>
public record struct RelayRequest(string Mode, string TenantName, string Target, string Url);

/// <summary>
/// Extension methods for the <see cref="HttpRequest"/>.
/// </summary>
public static class HttpRequestExtensions
{
	/// <summary>
	/// Parses the request url into a <see cref="RelayRequest"/>.
	/// </summary>
	/// <param name="httpRequest">A <see cref="HttpRequest"/> to be used for parsing.</param>
	/// <returns>The <see cref="RelayRequest"/>.</returns>
	public static RelayRequest GetRelayRequest(this HttpRequest httpRequest)
	{
		// /mode/tenant/target/(url)
		var parts = httpRequest.Path.Value?.Split('/').Skip(1).ToArray() ?? Array.Empty<string>();
		var mode = parts.ElementAt(0).ToLower();
		var tenantName = parts.ElementAtOrDefault(1) ?? string.Empty;
		var target = parts.ElementAtOrDefault(2) ?? string.Empty;
		var url = $"{string.Join("/", parts.Skip(3))}{httpRequest.QueryString}";

		return new RelayRequest(mode, tenantName, target, url);
	}
}
