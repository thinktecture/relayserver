namespace Thinktecture.Relay.Server.Transport;

internal class InMemoryTenantHandlerFactory : ITenantHandlerFactory
{
	public ITenantHandler Create(string tenantName, string connectionId, int maximumConcurrentRequests)
		=> InMemoryTenantHandler.Noop;
}
