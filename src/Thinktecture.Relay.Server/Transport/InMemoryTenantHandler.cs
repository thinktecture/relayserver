using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantHandler<TRequest> : ITenantHandler<TRequest>
		where TRequest : IClientRequest
	{
		public event AsyncEventHandler<TRequest>? RequestReceived;

		public InMemoryTenantHandler(Guid tenantId, ITenantDispatcher<TRequest> tenantDispatcher)
		{
			switch (tenantDispatcher)
			{
				case null:
					throw new ArgumentNullException(nameof(tenantDispatcher));

				case InMemoryTenantDispatcher<TRequest> inMemoryTenantDispatcher:
					inMemoryTenantDispatcher.RequestReceived += async (sender, request) =>
					{
						if (request.TenantId != tenantId)
						{
							return;
						}

						await RequestReceived.InvokeAsync(sender, request);
					};
					break;

				default:
					throw new ArgumentException($"The registered tenant dispatcher must be of type {nameof(InMemoryTenantDispatcher<TRequest>)}",
						nameof(tenantDispatcher));
			}
		}

		public Task AcknowledgeAsync(string acknowledgeId)
		{
			throw new NotImplementedException();
		}
	}
}
