using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Maintenance;

/// <summary>
/// An implementation of a job that will be instantiated and called from a background task in a configured maintenance
/// interval.
/// </summary>
public interface IMaintenanceJob
{
	/// <summary>
	/// Called when the maintenance interval
	/// </summary>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	Task DoMaintenanceAsync(CancellationToken cancellationToken = default);
}
