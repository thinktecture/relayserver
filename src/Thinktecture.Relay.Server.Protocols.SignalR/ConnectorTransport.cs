using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	// TODO discuss this if this should be instantiated for every connection instead
	internal class ConnectorTransport<TRequest, TResponse> : IConnectorTransport<TRequest>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> _hubContext;

		public ConnectorTransport(IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> hubContext)
			=> _hubContext = hubContext;

		public async Task RequestTargetAsync(TRequest request, string connectionId, CancellationToken cancellationToken = default)
			=> await _hubContext.Clients.Client(connectionId).RequestTarget(request);
	}
}
