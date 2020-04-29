using System.Threading.Tasks;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Repository that allows access to persisted tenants.
	/// </summary>
	public interface ITenantRepository
	{
		/// <summary>
		/// Loads a <see cref="Tenant"/> by its name.
		/// </summary>
		/// <param name="name">The name of the <see cref="Tenant"/> to load.</param>
		/// <returns>A tenant.</returns>
		Task<Tenant> LoadTenantByNameAsync(string name);
	}
}
