using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Connector
{
	/// <summary>
	/// An implementation of a registry for an <see cref="ITenantConnectorAdapter{TRequest}"/>. This mangles the communication
	/// between the underlying tenant transport and connector transport.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantConnectorAdapterRegistry<TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Registers the connection by creating an <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RegisterAsync(Guid tenantId, string connectionId);

		/// <summary>
		/// Unregisters the connection by destroying the corresponding <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task UnregisterAsync(string connectionId);
	}
}
