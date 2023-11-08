// ReSharper disable once CheckNamespace; (extension methods on ClaimsPrincipal namespace)
namespace System.Security.Claims;

/// <summary>
/// Extension methods for the <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
	/// <summary>
	/// Extracts the tenant name from the claim "client_id" of the <see cref="ClaimsPrincipal"/>.
	/// </summary>
	/// <param name="principal">A <see cref="ClaimsPrincipal"/> to extract the tenant name from.</param>
	/// <returns>The tenant name.</returns>
	public static string GetTenantName(this ClaimsPrincipal? principal)
		=> principal?.GetClaimValue("client_id") ?? string.Empty;

	/// <summary>
	/// Extracts the tenant display name from the claim "client_name" of the <see cref="ClaimsPrincipal"/>.
	/// </summary>
	/// <param name="principal">A <see cref="ClaimsPrincipal"/> to extract the tenant display name from.</param>
	/// <returns>The tenant display name, or null if not available.</returns>
	public static string? GetTenantDisplayName(this ClaimsPrincipal? principal)
		=> principal?.GetClaimValue("client_name");

	/// <summary>
	/// Extracts the tenant description from the claim "client_description" of the <see cref="ClaimsPrincipal"/>.
	/// </summary>
	/// <param name="principal">A <see cref="ClaimsPrincipal"/> to extract the tenant description from.</param>
	/// <returns>The tenant description, or null if not available.</returns>
	public static string? GetTenantDescription(this ClaimsPrincipal? principal)
		=> principal?.GetClaimValue("client_description");

	private static string? GetClaimValue(this ClaimsPrincipal principal, string claimName)
		=> principal.FindFirst(claimName)?.Value;
}
