using System;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantHandlerFactory : ITenantHandlerFactory
	{
		public ITenantHandler Create(Guid tenantId, string connectionId) => InMemoryTenantHandler.Noop;
	}
}
