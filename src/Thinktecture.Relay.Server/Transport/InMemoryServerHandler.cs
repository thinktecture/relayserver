using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryServerHandler<TResponse> : IServerHandler<TResponse>
		where TResponse : IRelayTargetResponse
	{
		public event AsyncEventHandler<TResponse> ResponseReceived;
		public event AsyncEventHandler<IAcknowledgeRequest> AcknowledgeReceived;

		public async Task DispatchResponseAsync(TResponse response) => await ResponseReceived.InvokeAsync(this, response);

		public async Task DispatchAcknowledgeAsync(IAcknowledgeRequest request) => await AcknowledgeReceived.InvokeAsync(this, request);
	}
}
