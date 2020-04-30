using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a transport from and to the connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IConnectorTransport<in TRequest, TResponse>
		where TRequest : ITransportClientRequest
		where TResponse : ITransportTargetResponse
	{
		/// <summary>
		/// Request the response from a target.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response from the target.</returns>
		Task<TResponse> RequestTargetAsync(TRequest request);
	}
}
