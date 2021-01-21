using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence
{
	/// <summary>
	/// Repository that allows access to fulfilled requests.
	/// </summary>
	public interface IRequestRepository
	{
		/// <summary>
		/// Stores the request.
		/// </summary>
		/// <param name="request">The <see cref="Request"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task StoreRequestAsync(Request request, CancellationToken cancellationToken);
	}
}
