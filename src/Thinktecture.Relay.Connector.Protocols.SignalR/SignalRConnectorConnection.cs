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
		private readonly HubConnection _connection;
		private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

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

			_connection.Closed += OnClosed;
			_connection.Reconnecting += OnReconnecting;
			_connection.Reconnected += OnReconnected;

			_clientRequestHandler.Acknowledge += OnAcknowledge;
		}

		private async Task OnClosed(Exception ex)
		{
			if (ex == null)
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

		private async Task OnReconnecting(Exception ex)
		{
			_logger.LogInformation("Trying to reconnect after connection was lost on connection {ConnectionId}", _connectionId);
			_logger.LogTrace(ex, "Reconnecting on {ConnectionId}", _connectionId);
			await Reconnecting.InvokeAsync(this, _connectionId);
		}

		private async Task OnReconnected(string connectionId)
		{
			_connectionId = connectionId;
			_logger.LogInformation("Reconnected on connection {ConnectionId}", _connectionId);
			await Reconnected.InvokeAsync(this, _connectionId);
		}

		private async Task OnAcknowledge(object sender, IAcknowledgeRequest request) => await AcknowledgeAsync(request);

		private async Task RequestTargetAsync(TRequest request)
		{
			_logger.LogTrace("Received request {RequestId} {@Request}", request.RequestId, request);
			_logger.LogDebug("Received request {RequestId} on connection {ConnectionId} from origin {OriginId}", request.RequestId,
				_connectionId, request.RequestOriginId);
			var response = await _clientRequestHandler.HandleAsync(request, BinarySizeThreshold);
			_logger.LogTrace("Received response for request {RequestId} {@Response}", request.RequestId, response);
			_logger.LogDebug("Received response for request {RequestId} on connection {ConnectionId}", request.RequestId, _connectionId);
			await DeliverAsync(response);
		}

		private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
		{
			_logger.LogTrace("Connecting");

			try
			{
				await _connection.StartAsync(cancellationToken);
				_connectionId = _connection.ConnectionId;
				_logger.LogInformation("Connected on connection {ConnectionId}", _connection.ConnectionId);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while trying to connect");
				await Task.Delay(5000, cancellationToken); // TODO make this configurable
				await ConnectAsyncInternal(cancellationToken);
			}
		}

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; } = 64 * 1024;

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
			return _connection.InvokeAsync("Deliver", response);
		}

		/// <inheritdoc />
		public Task AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Acknowledging request {@AcknowledgeRequest} on connection {ConnectionId}", request, _connectionId);
			return _connection.InvokeAsync("Acknowledge", request);
		}

		/// <inheritdoc />
		public Task PongAsync()
		{
			_logger.LogTrace("Pong on connection {ConnectionId}", _connectionId);
			return _connection.InvokeAsync("Pong");
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
			_logger.LogDebug("Disconnecting");

			_cancellationTokenSource.Cancel();
			await _connection.StopAsync(cancellationToken);
			_logger.LogInformation("Disconnected on connection {ConnectionId}", _connectionId);

			await Disconnected.InvokeAsync(this, _connectionId);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			await _connection.DisposeAsync();
			_cancellationTokenSource.Dispose();
			_clientRequestHandler.Acknowledge -= OnAcknowledge;
		}
	}
}
