using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	/// <inheritdoc cref="IConnectorConnection" />
	public class ConnectorConnection<TRequest, TResponse> : IConnectorConnection, IConnectorTransport<TResponse>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<ConnectorConnection<TRequest, TResponse>> _logger;
		private readonly DiscoveryDocumentRetryPolicy _retryPolicy;
		private readonly IClientRequestHandler<TRequest, TResponse> _clientRequestHandler;

		private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();
		private HubConnection? _connection;
		private string _connectionId = string.Empty;
		private bool? _enableTracing;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectorConnection{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="retryPolicy">The <see cref="DiscoveryDocumentRetryPolicy"/>.</param>
		/// <param name="hubConnectionFactory">The <see cref="HubConnectionFactory"/>.</param>
		/// <param name="clientRequestHandler">An <see cref="IClientRequestHandler{TRequest,TResponse}"/>.</param>
		public ConnectorConnection(ILogger<ConnectorConnection<TRequest, TResponse>> logger, DiscoveryDocumentRetryPolicy retryPolicy,
			HubConnectionFactory hubConnectionFactory, IClientRequestHandler<TRequest, TResponse> clientRequestHandler)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
			_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));

			_connection = hubConnectionFactory.Create() ?? throw new ArgumentNullException(nameof(hubConnectionFactory));
			_connection.On<TRequest>("RequestTarget", RequestTargetAsync);
			_connection.On<ITenantConfig>("Configure", ConfigureAsync);

			_connection.Closed += ConnectionClosed;
			_connection.Reconnecting += ConnectionReconnecting;
			_connection.Reconnected += ConnectionReconnected;

			_clientRequestHandler.AcknowledgeRequest += ClientRequestHandlerAcknowledgeRequest;
			_clientRequestHandler.DeliverResponse += ClientRequestHandlerDeliverResponse;
		}

		private async Task ConnectionClosed(Exception? ex)
		{
			if (ex == null || ex is OperationCanceledException)
			{
				_logger.LogDebug("Connection {ConnectionId} gracefully closed", _connectionId);
			}
			else
			{
				_logger.LogWarning(ex, "Connection {ConnectionId} closed", _connectionId);

				var token = _cancellationTokenSource?.Token;
				if (token == null) return;

				await Reconnecting.InvokeAsync(this, _connectionId);
				await ConnectAsyncInternal(token.Value);
				await Reconnected.InvokeAsync(this, _connectionId);
			}
		}

		private async Task ConnectionReconnecting(Exception ex)
		{
			_logger.LogInformation("Trying to reconnect after connection {ConnectionId} was lost", _connectionId);
			await Reconnecting.InvokeAsync(this, _connectionId);
		}

		private async Task ConnectionReconnected(string connectionId)
		{
			_logger.LogDebug("Reconnected on connection {ConnectionId}", connectionId);

			if (_connectionId != connectionId)
			{
				_logger.LogWarning("Dropped connection {ConnectionId} in favor of new connection {ConnectionId}", _connectionId, connectionId);
			}

			_connectionId = connectionId;
			await Reconnected.InvokeAsync(this, _connectionId);
		}

		private Task ClientRequestHandlerAcknowledgeRequest(object sender, IAcknowledgeRequest request) => AcknowledgeAsync(request);

		private Task ClientRequestHandlerDeliverResponse(object sender, TResponse response) => DeliverAsync(response);

		private async Task RequestTargetAsync(TRequest request)
		{
			_logger.LogTrace("Handling request {RequestId} on connection {ConnectionId} {@Request}", request.RequestId, _connectionId,
				request);
			_logger.LogDebug("Handling request {RequestId} on connection {ConnectionId} from origin {OriginId}", request.RequestId,
				_connectionId, request.RequestOriginId);

			request.EnableTracing = request.EnableTracing || _enableTracing.GetValueOrDefault();

			var token = _cancellationTokenSource?.Token;
			if (token == null) return;

			await _clientRequestHandler.HandleAsync(request, BinarySizeThreshold, token.Value);
		}

		private Task ConfigureAsync(ITenantConfig config)
		{
			_logger.LogTrace("Received tenant config {@Config} on connection {ConnectionId}", config, _connectionId);

			_connection?.SetKeepAliveInterval(config.KeepAliveInterval);
			_retryPolicy.SetReconnectDelays(config.ReconnectMinimumDelay, config.ReconnectMaximumDelay);

			if (config.EnableTracing != null)
			{
				_enableTracing = config.EnableTracing;
			}

			return Task.CompletedTask;
		}

		private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
		{
			if (_connection == null) return;

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
		public int? BinarySizeThreshold { get; } = 16 * 1024; // 16kb

		/// <inheritdoc />
		public event AsyncEventHandler<string>? Connected;

		/// <inheritdoc />
		public event AsyncEventHandler<string>? Reconnecting;

		/// <inheritdoc />
		public event AsyncEventHandler<string>? Reconnected;

		/// <inheritdoc />
		public event AsyncEventHandler<string>? Disconnected;

		/// <inheritdoc />
		public async Task DeliverAsync(TResponse response)
		{
			_logger.LogTrace("Delivering response for request {RequestId} on connection {ConnectionId}", response.RequestId, _connectionId);
			try
			{
				await _connection.InvokeAsync("Deliver", response);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while delivering response for request {RequestId} on connection {ConnectionId}",
					response.RequestId, _connectionId);
			}
		}

		/// <inheritdoc />
		public async Task AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Acknowledging request {@AcknowledgeRequest} on connection {ConnectionId}", request, _connectionId);
			try
			{
				await _connection.InvokeAsync("Acknowledge", request);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while acknowledging request {RequestId} on connection {ConnectionId}", request.RequestId,
					_connectionId);
			}
		}

		/// <inheritdoc />
		public async Task PongAsync()
		{
			_logger.LogTrace("Pong on connection {ConnectionId}", _connectionId);
			try
			{
				await _connection.InvokeAsync("Pong");
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while sending pong on connection {ConnectionId}", _connectionId);
			}
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

			_cancellationTokenSource?.Cancel();
			await _connection.StopAsync(cancellationToken);
			_logger.LogInformation("Disconnected on connection {ConnectionId}", _connectionId);

			await Disconnected.InvokeAsync(this, _connectionId);
		}

		/// <inheritdoc />
		public ValueTask DisposeAsync()
		{
			if (_connection == null && _cancellationTokenSource == null) return new ValueTask();

			lock (this)
			{
				var connection = _connection;
				_connection = null;

				if (connection != null)
				{
					connection.Closed -= ConnectionClosed;
					connection.Reconnecting -= ConnectionReconnecting;
					connection.Reconnected -= ConnectionReconnected;
					connection.DisposeAsync().GetAwaiter().GetResult();
				}

				var cancellationTokenSource = _cancellationTokenSource;
				_cancellationTokenSource = null;

				if (cancellationTokenSource != null)
				{
					cancellationTokenSource.Cancel();
					cancellationTokenSource.Dispose();
				}

				_clientRequestHandler.AcknowledgeRequest -= ClientRequestHandlerAcknowledgeRequest;
				_clientRequestHandler.DeliverResponse -= ClientRequestHandlerDeliverResponse;
			}

			return new ValueTask();
		}
	}
}
