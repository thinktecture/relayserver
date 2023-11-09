using System;

namespace Thinktecture.Relay.Server.Transport;

/// <summary>
/// An implementation of a factory to create an instance if a class implementing <see cref="ITenantHandler"/>.
/// </summary>
public interface ITenantHandlerFactory
{
	/// <summary>
	/// Creates an instance of a class implementing <see cref="ITenantHandler"/>.
	/// </summary>
	/// <param name="tenantName">The unique name of the tenant.</param>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <returns>An <see cref="ITenantHandler"/>.</returns>
	ITenantHandler Create(string tenantName, string connectionId);
}
