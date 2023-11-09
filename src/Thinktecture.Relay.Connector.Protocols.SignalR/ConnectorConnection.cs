using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

/// <inheritdoc cref="IConnectorConnection"/>
public partial class ConnectorConnection<TRequest, TResponse, TAcknowledge> : IConnectorConnection, IDisposable
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IClientRequestHandler<TRequest> _clientRequestHandler;
	private readonly ILogger _logger;

	private readonly DiscoveryDocumentRetryPolicy _retryPolicy;

	private CancellationTokenSource? _cancellationTokenSource = new CancellationTokenSource();
	private string _connectionId = string.Empty;
	private bool? _enableTracing;
	private HubConnection? _hubConnection;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectorConnection{TRequest,TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="retryPolicy">The <see cref="DiscoveryDocumentRetryPolicy"/>.</param>
	/// <param name="clientRequestHandler">An <see cref="IClientRequestHandler{T}"/>.</param>
	/// <param name="hubConnection">The <see cref="HubConnection"/>.</param>
	public ConnectorConnection(ILogger<ConnectorConnection<TRequest, TResponse, TAcknowledge>> logger,
		DiscoveryDocumentRetryPolicy retryPolicy, IClientRequestHandler<TRequest> clientRequestHandler,
		HubConnection hubConnection)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_retryPolicy = retryPolicy ?? throw new ArgumentNullException(nameof(retryPolicy));
		_clientRequestHandler = clientRequestHandler ?? throw new ArgumentNullException(nameof(clientRequestHandler));

		_hubConnection = hubConnection ?? throw new ArgumentNullException(nameof(hubConnection));
		_hubConnection.On<TRequest>("RequestTarget", RequestTargetAsync);
		_hubConnection.On<ITenantConfig>("Configure", ConfigureAsync);

		_hubConnection.Closed += HubConnectionClosed;
		_hubConnection.Reconnecting += HubConnectionReconnecting;
		_hubConnection.Reconnected += HubConnectionReconnected;
	}

	/// <inheritdoc />
	public event AsyncEventHandler<string>? Connected;

	/// <inheritdoc />
	public event AsyncEventHandler<string>? Reconnecting;

	/// <inheritdoc />
	public event AsyncEventHandler<string>? Reconnected;

	/// <inheritdoc />
	public event AsyncEventHandler<string>? Disconnected;

	/// <inheritdoc />
	public async Task ConnectAsync(CancellationToken cancellationToken)
	{
		await ConnectAsyncInternal(cancellationToken);
		await Connected.InvokeAsync(this, _connectionId);
	}

	/// <inheritdoc />
	public async Task DisconnectAsync(CancellationToken cancellationToken)
	{
		if (_hubConnection is null) return;

		Log.Disconnecting(_logger, _connectionId);

		if (_cancellationTokenSource is not null)
		{
			await _cancellationTokenSource.CancelAsync();
		}
		await _hubConnection.StopAsync(cancellationToken);

		Log.Disconnected(_logger, _connectionId);
		await Disconnected.InvokeAsync(this, _connectionId);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		if (_hubConnection is null && _cancellationTokenSource is null) return;

		lock (this)
		{
			var connection = _hubConnection;
			_hubConnection = null;

			if (connection is not null)
			{
				connection.Closed -= HubConnectionClosed;
				connection.Reconnecting -= HubConnectionReconnecting;
				connection.Reconnected -= HubConnectionReconnected;
				connection.DisposeAsync().GetAwaiter().GetResult();
			}

			var cancellationTokenSource = _cancellationTokenSource;
			_cancellationTokenSource = null;

			cancellationTokenSource?.Cancel();
			cancellationTokenSource?.Dispose();
		}
	}

	private async Task HubConnectionClosed(Exception? ex)
	{
		if (ex is null or OperationCanceledException)
		{
			Log.ConnectionClosedGracefully(_logger, _connectionId);
		}
		else
		{
			Log.ConnectionClosed(_logger, _connectionId);

			var token = _cancellationTokenSource?.Token;
			if (token is null) return;

			await Reconnecting.InvokeAsync(this, _connectionId);
			await ConnectAsyncInternal(token.Value);
			await Reconnected.InvokeAsync(this, _connectionId);
		}
	}

	private async Task HubConnectionReconnecting(Exception? ex)
	{
		if (ex is null)
		{
			Log.ReconnectingAfterLoss(_logger, _connectionId);
		}
		else
		{
			Log.ReconnectingAfterError(_logger, ex, _connectionId);
		}
		await Reconnecting.InvokeAsync(this, _connectionId);
	}

	private async Task HubConnectionReconnected(string? connectionId)
	{
		if (connectionId is null)
		{
			Log.ReconnectedWithoutId(_logger);
		}
		else if (_connectionId == connectionId)
		{
			Log.Reconnected(_logger, _connectionId);
		}
		else
		{
			Log.ReconnectedWithNewId(_logger, _connectionId, connectionId);
			_connectionId = connectionId;
		}

		await Reconnected.InvokeAsync(this, _connectionId);
	}

	private async Task RequestTargetAsync(TRequest request)
	{
		Log.HandlingRequestDetailed(_logger, request.RequestId, _connectionId, request);
		Log.HandlingRequestSimple(_logger, request.RequestId, _connectionId, request.RequestOriginId);

		request.EnableTracing = request.EnableTracing || _enableTracing.GetValueOrDefault();

		var token = _cancellationTokenSource?.Token;
		if (token is null) return;

		await _clientRequestHandler.HandleAsync(request, token.Value);
	}

	private Task ConfigureAsync(ITenantConfig config)
	{
		Log.ReceivedTenantConfig(_logger, config, _connectionId);

		_hubConnection?.SetKeepAliveInterval(config.KeepAliveInterval);
		_retryPolicy.SetReconnectDelays(config.ReconnectMinimumDelay, config.ReconnectMaximumDelay);

		if (config.EnableTracing is not null)
		{
			_enableTracing = config.EnableTracing;
		}

		return Task.CompletedTask;
	}

	private async Task ConnectAsyncInternal(CancellationToken cancellationToken)
	{
		if (_hubConnection is null) return;

		try
		{
			await _hubConnection.StartAsync(cancellationToken);
			_connectionId = _hubConnection.ConnectionId!;

			Log.LogConnected(_logger, _connectionId);
		}
		catch (OperationCanceledException)
		{
			// ignore this, as this will be thrown when the service shuts down gracefully
		}
		catch (Exception ex)
		{
			// due to the retry policy this should never be caught
			Log.ConnectError(_logger, ex);
			await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
			await ConnectAsyncInternal(cancellationToken);
		}
	}
}
