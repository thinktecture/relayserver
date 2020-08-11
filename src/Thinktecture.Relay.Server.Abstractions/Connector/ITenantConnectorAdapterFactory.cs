using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Connector
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITenantConnectorAdapter{TRequest}"/>.
	/// </summary>
	public interface ITenantConnectorAdapterFactory<in TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <returns>An <see cref="ITenantConnectorAdapter{TRequest}"/>.</returns>
		ITenantConnectorAdapter<TRequest> Create(Guid tenantId, string connectionId);
	}
}
