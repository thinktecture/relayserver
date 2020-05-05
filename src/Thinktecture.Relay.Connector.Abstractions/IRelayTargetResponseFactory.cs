using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="IRelayTargetResponse"/>.
	/// </summary>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IRelayTargetResponseFactory<TResponse>
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="IRelayTargetResponse"/> from <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="HttpResponseMessage"/> from the target.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the creation of an instance
		/// implementing <see cref="IRelayTargetResponse"/>.</returns>
		/// <remarks>Some properties will always be set on the result after calling this method (e.g. RequestId).</remarks>
		Task<TResponse> CreateAsync(HttpResponseMessage message);
	}
}
