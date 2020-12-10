using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	/// <inheritdoc cref="IConnectorConnection" />
	public class SignalRConnectorConnection<TRequest, TResponse> : IConnectorConnection, IConnectorTransport<TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<SignalRConnectorConnection<TRequest, TResponse>> _logger;
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;

		private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
		private HubConnection _connection;
		private string _connectionId;

		/// <summary>
		/// Initializes a new instance of the <see cref="SignalRConnectorConnection{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="signalRConnectionFactory">The <see cref="SignalRConnectionFactory"/>.</param>
		/// <param name="clientRequestHandler">An <see cref="IClientRequestHandler{TRequest,TResponse}"/>.</param>
		public SignalRConnectorConnection(ILogger<SignalRConnectorConnection<TRequest, TResponse>> logger,
			SignalRConnectionFactory signalRConnectionFactory, IClientRequestHandler<TRequest, TResponse> clientRequestHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));

			_connection = signalRConnectionFactory?.CreateConnection() ?? throw new ArgumentNullException(nameof(signalRConnectionFactory));
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);

			_connection.Closed += ConnectionClosed;
			_connection.Reconnecting += ConnectionReconnecting;
			_connection.Reconnected += ConnectionReconnected;

			_clientRequestHandler.Acknowledge += ClientRequestHandlerAcknowledge;
		}

		private async Task ConnectionClosed(Exception ex)
		{
			if (ex == null || ex is OperationCanceledException)
			{
				_logger.LogDebug("Connection {ConnectionId} gracefully closed", _connectionId);
			}
			else
			{
				_logger.LogError(ex, "Connection {ConnectionId} closed", _connectionId);

				await Reconnecting.InvokeAsync(this, _connectionId);
				await ConnectAsyncInternal(_cancellationTokenSource.Token);
				await Reconnected.InvokeAsync(this, _connectionId);
			}
		}

		private async Task ConnectionReconnecting(Exception ex)
		{
			_logger.LogInformation("Trying to reconnect after connection was lost on connection {ConnectionId}", _connectionId);
			await Reconnecting.InvokeAsync(this, _connectionId);
		}

		private async Task ConnectionReconnected(string connectionId)
		{
			_logger.LogInformation("Reconnected on connection {ConnectionId} as {ConnectionId}", _connectionId, connectionId);
			_connectionId = connectionId;
			await Reconnected.InvokeAsync(this, _connectionId);
		}

		private async Task ClientRequestHandlerAcknowledge(object sender, IAcknowledgeRequest request) => await AcknowledgeAsync(request);

		private async Task RequestTargetAsync(TRequest request)
		{
			_logger.LogTrace("Handling request {RequestId} on connection {ConnectionId} {@Request}", request.RequestId, _connectionId,
				request);
			_logger.LogDebug("Handling request {RequestId} on connection {ConnectionId} from origin {OriginId}", request.RequestId,
				_connectionId, request.RequestOriginId);

			var response = await _clientRequestHandler.HandleAsync(request, BinarySizeThreshold);

			_logger.LogTrace("Received response for request {RequestId} on connection {ConnectionId} {@Response}", request.RequestId,
				_connectionId, response);
			_logger.LogDebug("Received response for request {RequestId}", request.RequestId);

			await DeliverAsync(response);
		}

		private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
		{
			if (_connection == null) return;

			_logger.LogTrace("Connecting");

			try
			{
				await _connection.StartAsync(cancellationToken);
				_connectionId = _connection.ConnectionId;

				_logger.LogInformation("Connected on connection {ConnectionId}", _connection.ConnectionId);
			}
			catch (OperationCanceledException)
			{
				// Ignore this, as this will be thrown when the service shuts down gracefully
			}
			catch (Exception ex)
			{
				// due to the retry policy this should never be caught
				_logger.LogError(ex, "An error occured while trying to connect");
				await Task.Delay(1000, cancellationToken);
				await ConnectAsyncInternal(cancellationToken);
			}
		}

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; } = 32 * 1024; // 32kb

		/// <inheritdoc />
		public event AsyncEventHandler<string> Connected;

		/// <inheritdoc />
		public event AsyncEventHandler<string> Reconnecting;

		/// <inheritdoc />
		public event AsyncEventHandler<string> Reconnected;

		/// <inheritdoc />
		public event AsyncEventHandler<string> Disconnected;

		/// <inheritdoc />
		public Task DeliverAsync(TResponse response)
		{
			_logger.LogTrace("Delivering response for request {RequestId} on connection {ConnectionId}", response.RequestId, _connectionId);
			return _connection?.InvokeAsync("Deliver", response);
		}

		/// <inheritdoc />
		public Task AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Acknowledging request {@AcknowledgeRequest} on connection {ConnectionId}", request, _connectionId);
			return _connection?.InvokeAsync("Acknowledge", request);
		}

		/// <inheritdoc />
		public Task PongAsync()
		{
			_logger.LogTrace("Pong on connection {ConnectionId}", _connectionId);
			return _connection?.InvokeAsync("Pong");
		}

		/// <inheritdoc />
		public async Task ConnectAsync(CancellationToken cancellationToken)
		{
			await ConnectAsyncInternal(cancellationToken);
			await Connected.InvokeAsync(this, _connectionId);
		}

		/// <inheritdoc />
		public async Task DisconnectAsync(CancellationToken cancellationToken)
		{
			if (_connection == null) return;

			_logger.LogTrace("Disconnecting connection {ConnectionId}", _connectionId);

			_cancellationTokenSource.Cancel();
			await _connection.StopAsync(cancellationToken);
			_logger.LogInformation("Disconnected on connection {ConnectionId}", _connectionId);

			await Disconnected.InvokeAsync(this, _connectionId);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			if (_connection != null)
			{
				await _connection.DisposeAsync();
				_connection.Closed -= ConnectionClosed;
				_connection.Reconnecting -= ConnectionReconnecting;
				_connection.Reconnected -= ConnectionReconnected;
				_connection = null;
			}

			_cancellationTokenSource?.Cancel();
			_cancellationTokenSource?.Dispose();
			_cancellationTokenSource = null;

			_clientRequestHandler.Acknowledge -= ClientRequestHandlerAcknowledge;
		}
	}
}
