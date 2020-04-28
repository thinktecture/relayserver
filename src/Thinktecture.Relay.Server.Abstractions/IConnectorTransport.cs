using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server.Abstractions
{
	/// <summary>
	/// An implementation of a transport from and to the connector.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface IConnectorTransport<in TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Request the response from a target
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		Task<TResponse> RequestTargetAsync(TRequest request);
	}
}
