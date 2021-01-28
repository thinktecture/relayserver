using Thinktecture.Relay.Server.Security;

// ReSharper disable once CheckNamespace; (extension methods on ClaimsPrincipal namespace)
namespace System.Security.Claims
{
	/// <summary>
	/// Extension methods for the <see cref="ClaimsPrincipal"/>.
	/// </summary>
	public static class ClaimsPrincipalExtensions
	{
		/// <summary>
		/// Creates a <see cref="TenantInfo"/> object based on a given <see cref="ClaimsPrincipal"/>.
		/// </summary>
		/// <param name="principal">The <see cref="ClaimsPrincipal"/> to extract the claims from.</param>
		/// <returns>A new instance of a parsed <see cref="TenantInfo"/> object.</returns>
		/// <exception cref="ArgumentNullException">Thrown when no <see cref="ClaimsPrincipal"/> is provided.</exception>
		public static TenantInfo GetTenantInfo(this ClaimsPrincipal principal)
		{
			if (principal == null) throw new ArgumentNullException(nameof(principal));

			return new TenantInfo()
			{
				Id = Guid.Parse(principal.FindFirst("client_id").Value),
				Name = principal.FindFirst("client_name").Value,
				DisplayName = principal.FindFirst("client_display_name").Value
			};
		}
	}
}
