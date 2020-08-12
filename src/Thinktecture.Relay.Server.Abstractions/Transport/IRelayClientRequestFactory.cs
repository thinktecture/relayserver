using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="IRelayClientRequest"/>.
	/// </summary>
	public interface IRelayClientRequestFactory<TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="IRelayClientRequest"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the creation of an instance implementing <see cref="IRelayClientRequest"/>.</returns>
		Task<TRequest> CreateAsync(Guid tenantId);
	}
}
