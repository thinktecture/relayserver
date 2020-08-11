using System.Threading.Tasks;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryServerDispatcher<TResponse> : IServerDispatcher<TResponse>
		where TResponse : IRelayTargetResponse
	{
		private readonly InMemoryServerHandler<TResponse> _serverHandler;

		public InMemoryServerDispatcher(InMemoryServerHandler<TResponse> serverHandler) => _serverHandler = serverHandler;

		public async Task DispatchResponseAsync(TResponse response) => await _serverHandler.DispatchResponseAsync(response);

		public async Task DispatchAcknowledgeAsync(IAcknowledgeRequest request) => await _serverHandler.DispatchAcknowledgeAsync(request);
	}
}
