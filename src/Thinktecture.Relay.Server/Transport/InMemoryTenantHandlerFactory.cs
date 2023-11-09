using System;

namespace Thinktecture.Relay.Server.Transport;

internal class InMemoryTenantHandlerFactory : ITenantHandlerFactory
{
	public ITenantHandler Create(string tenantName, string connectionId)
		=> InMemoryTenantHandler.Noop;
}
