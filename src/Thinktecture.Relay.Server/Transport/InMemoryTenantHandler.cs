using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantHandler<TRequest> : ITenantHandler<TRequest>
		where TRequest : IRelayClientRequest
	{
		public event AsyncEventHandler<TRequest> RequestReceived;

		public InMemoryTenantHandler(Guid tenantId, ITenantDispatcher<TRequest> tenantDispatcher)
		{
			switch (tenantDispatcher)
			{
				case null:
					throw new ArgumentNullException(nameof(tenantDispatcher));

				case InMemoryTenantDispatcher<TRequest> inMemoryTenantDispatcher:
					inMemoryTenantDispatcher.RequestReceived += async (sender, @event) =>
					{
						if (@event.TenantId != tenantId)
						{
							return;
						}

						await RequestReceived.InvokeAsync(sender, @event);
					};
					break;

				default:
					throw new ArgumentException($"The registered tenant dispatcher must be of type {nameof(InMemoryTenantDispatcher<TRequest>)}",
						nameof(tenantDispatcher));
			}
		}
	}
}
