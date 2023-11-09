using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Represents a way to access connection data in the persistence layer.
/// </summary>
public interface IConnectionService
{
	/// <summary>
	/// Returns true when a connection for the tenant is available; otherwise, false.
	/// </summary>
	/// <param name="tenantName">The unique name of the tenant.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the result.</returns>
	Task<bool> IsConnectionAvailableAsync(string tenantName);
}
