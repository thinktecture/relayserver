using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	internal interface IConnector<in TRequest>
		where TRequest : IRelayClientRequest
	{
		Task RequestTarget(TRequest request);
	}

	internal class ConnectorHub<TRequest, TResponse> : Hub<IConnector<TRequest>>, Connector.IConnectorTransport
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly ITenantConnectorAdapterLocator<TRequest, TResponse> _tenantConnectorAdapterLocator;
		private readonly IServerDispatcher<TResponse> _serverDispatcher;

		public ConnectorHub(ITenantConnectorAdapterLocator<TRequest, TResponse> tenantConnectorAdapterLocator,
			IServerDispatcher<TResponse> serverDispatcher)
		{
			_tenantConnectorAdapterLocator = tenantConnectorAdapterLocator;
			_serverDispatcher = serverDispatcher;
		}

		public override async Task OnConnectedAsync()
		{
			// TODO retrieve tenant id from somewhere (e.g. Claims?)
			await _tenantConnectorAdapterLocator.RegisterAdapterAsync(Guid.Empty, Context.ConnectionId);
			await base.OnConnectedAsync();
		}

		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await _tenantConnectorAdapterLocator.UnregisterAdapterAsync(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}

		[HubMethodName("Acknowledge")]
		public async Task AcknowledgeAsync(IAcknowledgeRequest request) => await _serverDispatcher.DispatchAcknowledgeAsync(request);

		[HubMethodName("Pong")]
		public Task PongAsync()
		{
			throw new NotImplementedException();
		}
	}
}
