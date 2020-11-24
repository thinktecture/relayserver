using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Maintenance
{
	/// <summary>
	/// An implementation of a job that will be instanciated and called from a background task in a configured maintenance interval.
	/// </summary>
	public interface IMaintenanceJob
	{
		/// <summary>
		/// Called when the maintenance interval
		/// </summary>
		/// <param name="stoppingToken">Indicates that the maintenance loop has been aborted.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DoMaintenanceAsync(CancellationToken stoppingToken);
	}
}
