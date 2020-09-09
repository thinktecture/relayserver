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
	public class ConnectorConnection<TRequest, TResponse> : IConnectorConnection, IConnectorTransport<TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<ConnectorConnection<TRequest, TResponse>> _logger;
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;
		private readonly HubConnection _connection;

		private IConnectorTransport<TResponse> Transport => this;
		private string _connectionId;

		public ConnectorConnection(ILogger<ConnectorConnection<TRequest, TResponse>> logger, ConnectionFactory connectionFactory,
			IClientRequestHandler<TRequest, TResponse> clientRequestHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));

			_connection = connectionFactory?.CreateConnection() ?? throw new ArgumentNullException(nameof(connectionFactory));
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);

			_connection.Closed += OnClosed;
			_connection.Reconnecting += OnReconnecting;
			_connection.Reconnected += OnReconnected;

			_clientRequestHandler.Acknowledge += OnAcknowledge;
		}

		private Task OnClosed(Exception ex)
		{
			if (ex == null)
			{
				_logger.LogDebug("Connection {ConnectionId} gracefully closed", _connectionId);
			}
			else
			{
				_logger.LogError(ex, "Connection {ConnectionId} closed", _connectionId);
			}
			return Task.CompletedTask;
		}

		private Task OnReconnecting(Exception ex)
		{
			_logger.LogInformation(ex, "Trying to reconnect after connection was lost on connection {ConnectionId}", _connectionId);
			_logger.LogTrace(ex, "Reconnecting on {ConnectionId}", _connectionId);
			return Task.CompletedTask;
		}

		private Task OnReconnected(string connectionId)
		{
			_connectionId = connectionId;
			_logger.LogInformation("Reconnected on connection {ConnectionId}", _connectionId);
			return Task.CompletedTask;
		}

		private async Task OnAcknowledge(object sender, IAcknowledgeRequest request) => await Transport.AcknowledgeAsync(request);

		private async Task RequestTargetAsync(TRequest request)
		{
			_logger.LogTrace("Received request {@Request} on connection {ConnectionId}", request, _connectionId);
			var response = await _clientRequestHandler.HandleAsync(request, Transport.BinarySizeThreshold);
			_logger.LogTrace("Received response {@Response} on connection {ConnectionId}", response, _connectionId);
			await Transport.DeliverAsync(response);
		}

		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 64 * 1024;

		Task IConnectorTransport<TResponse>.DeliverAsync(TResponse response)
		{
			_logger.LogTrace("Delivering response {@Response} on connection {ConnectionId}", response, _connectionId);
			return _connection.InvokeAsync("Deliver", response);
		}

		Task IConnectorTransport<TResponse>.AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Acknowledging request {@AcknowledgeRequest} on connection {ConnectionId}", request, _connectionId);
			return _connection.InvokeAsync("Acknowledge", request);
		}

		Task IConnectorTransport<TResponse>.PongAsync()
		{
			_logger.LogTrace("Pong on connection {ConnectionId}",  _connectionId);
			return _connection.InvokeAsync("Pong");
		}

		async Task IConnectorConnection.ConnectAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Connecting");
			await _connection.StartAsync(cancellationToken);
			_connectionId = _connection.ConnectionId;
			_logger.LogInformation("Connected on connection {ConnectionId}", _connection.ConnectionId);
		}

		async Task IConnectorConnection.DisconnectAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("Disconnecting");
			await _connection.StopAsync(cancellationToken);
			_logger.LogInformation("Disconnected on connection {ConnectionId}", _connectionId);
		}

		public async ValueTask DisposeAsync()
		{
			await _connection.DisposeAsync();
			_clientRequestHandler.Acknowledge -= OnAcknowledge;
		}
	}
}
