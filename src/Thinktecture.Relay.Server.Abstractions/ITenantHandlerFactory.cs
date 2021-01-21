using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITenantHandler{TRequest}"/>.
	/// </summary>
	public interface ITenantHandlerFactory<out TRequest>
		where TRequest : IClientRequest
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="ITenantHandler{TRequest}"/> for the tenant.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <returns>An <see cref="ITenantHandler{TRequest}"/>.</returns>
		ITenantHandler<TRequest> Create(Guid tenantId, string connectionId);
	}
}
