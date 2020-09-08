using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.RelayTargets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	internal class ServerConnection<TRequest, TResponse> : IConnectorConnection, IConnectorTransport<TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;
		private readonly ILogger<ServerConnection<TRequest, TResponse>> _logger;
		private readonly HubConnection _connection;

		private IConnectorTransport<TResponse> Transport => this;

		public ServerConnection(IClientRequestHandler<TRequest, TResponse> clientRequestHandler, ConnectionFactory connectionFactory,
			ILogger<ServerConnection<TRequest, TResponse>> logger)
		{
			if (connectionFactory == null)
			{
				throw new ArgumentNullException(nameof(connectionFactory));
			}

			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			_clientRequestHandler.Acknowledge += OnAcknowledge;

			_connection = connectionFactory.CreateConnection();
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);
		}

		private async Task OnAcknowledge(object sender, IAcknowledgeRequest request) => await Transport.AcknowledgeAsync(request);

		private async Task RequestTargetAsync(TRequest request)
		{
			_logger.LogTrace("Received request {@Request}", request);
			var response = await _clientRequestHandler.HandleAsync(request, Transport.BinarySizeThreshold);
			_logger.LogTrace("Received response {@Response}", response);
			await Transport.DeliverAsync(response);
		}

		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 64 * 1024;

		Task IConnectorTransport<TResponse>.DeliverAsync(TResponse response)
		{
			_logger.LogTrace("Delivering response {@Response}", response);
			return _connection.InvokeAsync("Deliver", response);
		}

		Task IConnectorTransport<TResponse>.AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Acknowledging request {@AcknowledgeRequest}", request);
			return _connection.InvokeAsync("Acknowledge", request.RequestId);
		}

		Task IConnectorTransport<TResponse>.PongAsync()
		{
			_logger.LogTrace("Pong");
			return _connection.InvokeAsync("Pong");
		}

		async Task IConnectorConnection.ConnectAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Connecting");
			await _connection.StartAsync(cancellationToken);
			_logger.LogInformation("Connected");
		}

		async Task IConnectorConnection.DisconnectAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Disconnecting");
			await _connection.StopAsync(cancellationToken);
			_logger.LogInformation("Disconnected");
		}

		public async ValueTask DisposeAsync()
		{
			await _connection.DisposeAsync();
			_clientRequestHandler.Acknowledge -= OnAcknowledge;
		}
	}
}
