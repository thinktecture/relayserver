using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a factory to create an instance of a class implementing <see cref="ITenantHandler{TRequest,TResponse}"/>.
	/// </summary>
	public interface ITenantHandlerFactory<out TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Creates an instance of a class implementing <see cref="ITenantHandler{TRequest,TResponse}"/> for the tenant.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <returns>An <see cref="ITenantHandler{TRequest,TResponse}"/>.</returns>
		ITenantHandler<TRequest, TResponse> Create(Guid tenantId);
	}
}
