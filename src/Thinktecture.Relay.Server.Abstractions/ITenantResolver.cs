using System.Threading.Tasks;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a resolver for tenants.
	/// </summary>
	public interface ITenantResolver
	{
		/// <summary>
		/// Resolves a tenant identity to a tenant.
		/// </summary>
		/// <param name="identity">The identity of the tenant.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the tenant or null
		/// if the identity is unknown.</returns>
		Task<object> ResolveAsync(object identity);
		// TODO replace with model and identity
	}
}
