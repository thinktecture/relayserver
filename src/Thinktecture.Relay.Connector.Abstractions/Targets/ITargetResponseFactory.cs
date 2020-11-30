using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Targets
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITargetResponse"/>.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface ITargetResponseFactory<TResponse>
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="ITargetResponse"/> from <paramref name="request"/> and <paramref name="message"/>.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <param name="message">The <see cref="HttpResponseMessage"/>.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the creation of an instance
		/// implementing <see cref="ITargetResponse"/>.</returns>
		Task<TResponse> CreateAsync(IClientRequest request, HttpResponseMessage message, CancellationToken cancellationToken = default);
	}
}
