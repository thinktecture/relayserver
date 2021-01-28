using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryTenantDispatcher<TRequest> : ITenantDispatcher<TRequest>
		where TRequest : IClientRequest
	{
		public event AsyncEventHandler<TRequest>? RequestReceived;

		public int? BinarySizeThreshold { get; } = null;

		public async Task DispatchRequestAsync(TRequest request) => await RequestReceived.InvokeAsync(this, request);
	}
}
