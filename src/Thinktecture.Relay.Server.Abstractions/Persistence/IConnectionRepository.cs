using System;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Persistence;

/// <summary>
/// Repository that allows access to connections.
/// </summary>
public interface IConnectionRepository
{
	/// <summary>
	/// Returns true when a connection for the tenant is available; otherwise, false.
	/// </summary>
	/// <param name="tenantId">The unique id of the tenant.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the result.</returns>
	Task<bool> IsConnectionAvailableAsync(Guid tenantId);
}
