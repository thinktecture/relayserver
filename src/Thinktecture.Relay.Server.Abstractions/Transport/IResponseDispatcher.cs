using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a dispatcher for target responses.
	/// </summary>
	/// <typeparam name="T">The type of response.</typeparam>
	public interface IResponseDispatcher<in T>
		where T : ITargetResponse
	{
		/// <summary>
		/// Dispatches the target response.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchAsync(T response, CancellationToken cancellationToken = default);
	}
}
