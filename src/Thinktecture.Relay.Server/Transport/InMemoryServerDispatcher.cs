using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryServerDispatcher<TResponse> : IServerDispatcher<TResponse>
		where TResponse : ITargetResponse
	{
		public event AsyncEventHandler<TResponse>? ResponseReceived;
		public event AsyncEventHandler<IAcknowledgeRequest>? AcknowledgeReceived;

		public int? BinarySizeThreshold { get; } = null;

		public async Task DispatchResponseAsync(TResponse response) => await ResponseReceived.InvokeAsync(this, response);

		public async Task DispatchAcknowledgeAsync(IAcknowledgeRequest request) => await AcknowledgeReceived.InvokeAsync(this, request);
	}
}
