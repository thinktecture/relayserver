using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantHandlerFactory<TRequest> : ITenantHandlerFactory<TRequest>
		where TRequest : IClientRequest
	{
		private readonly ITenantDispatcher<TRequest> _tenantDispatcher;

		public InMemoryTenantHandlerFactory(ITenantDispatcher<TRequest> tenantDispatcher)
		{
			_tenantDispatcher = tenantDispatcher ?? throw new ArgumentNullException(nameof(tenantDispatcher));
		}

		public ITenantHandler<TRequest> Create(Guid tenantId, string connectionId)
			=> new InMemoryTenantHandler<TRequest>(tenantId, _tenantDispatcher);
	}
}
