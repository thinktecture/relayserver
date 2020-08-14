using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	internal class ServerConnection<TRequest, TResponse> : IConnectorConnection, IConnectorTransport<TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;
		private readonly HubConnection _connection;

		private IConnectorTransport<TResponse> Transport => this;

		public ServerConnection(IClientRequestHandler<TRequest, TResponse> clientRequestHandler, SignalRConnectionFactory connectionFactory)
		{
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));

			_connection = connectionFactory.CreateConnection();
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);
		}

		private async Task RequestTargetAsync(TRequest request)
		{
			var response = await _clientRequestHandler.HandleAsync(request, Transport.BinarySizeThreshold);
			await Transport.DeliverAsync(response);
		}

		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 64 * 1024;

		Task IConnectorTransport<TResponse>.DeliverAsync(TResponse response)
			=> _connection.InvokeAsync("Deliver", response);

		Task IConnectorTransport<TResponse>.AcknowledgeAsync(IAcknowledgeRequest request)
			=> _connection.InvokeAsync("Acknowledge", request);

		Task IConnectorTransport<TResponse>.PongAsync()
			=> _connection.InvokeAsync("Pong");

		Task IConnectorConnection.ConnectAsync(CancellationToken cancellationToken)
			=> _connection.StartAsync(cancellationToken);

		Task IConnectorConnection.DisconnectAsync(CancellationToken cancellationToken)
			=> _connection.StopAsync(cancellationToken);
	}
}
