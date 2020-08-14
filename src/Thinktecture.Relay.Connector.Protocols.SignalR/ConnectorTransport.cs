using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	public class ServerConnection<TRequest, TResponse> : IConnectorTransport<TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;
		private HubConnection _connection;

		private IConnectorTransport<TResponse> Transport => this;

		public ServerConnection(IClientRequestHandler<TRequest, TResponse> clientRequestHandler)
		{
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));
		}

		// Todo: Create and open message

		private void RegisterIncomingMessages()
		{
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);
		}

		private async Task RequestTargetAsync(TRequest request)
		{
			var response = await _clientRequestHandler.HandleAsync(request);
			await Transport.DeliverAsync(response);
		}

		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 64 * 1024;

		Task IConnectorTransport<TResponse>.DeliverAsync(TResponse response)
			=> _connection.InvokeAsync("Deliver", response);

		Task IConnectorTransport<TResponse>.AcknowledgeAsync(IAcknowledgeRequest request)
			=> _connection.InvokeAsync("Acknowledge", request);

		Task IConnectorTransport<TResponse>.PongAsync()
			=> _connection.InvokeAsync("Pong");
	}
}
