using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.DependencyInjection
{
	/// <summary>
	/// Connector builder interface.
	/// </summary>
	public interface IRelayConnectorBuilder<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <summary>
		/// Gets the application service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}
