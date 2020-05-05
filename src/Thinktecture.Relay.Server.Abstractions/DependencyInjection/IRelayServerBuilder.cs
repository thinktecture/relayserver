using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.DependencyInjection
{
	/// <summary>
	/// Server builder interface.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface IRelayServerBuilder<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Gets the application service collection.
		/// </summary>
		IServiceCollection Services { get; }
	}
}
