using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

/// <inheritdoc />
public class ResponseTransport<T> : IResponseTransport<T>
	where T : ITargetResponse
{
	private readonly HubConnection _hubConnection;
	private readonly ILogger<ResponseTransport<T>> _logger;

	// TODO move to LoggerMessage source generator when destructuring is supported
	// (see https://github.com/dotnet/runtime/issues/69490)
	private readonly Action<ILogger, ITargetResponse, Guid, string?, Exception?> _logTransportingResponse =
		LoggerMessage.Define<ITargetResponse, Guid, string?>(LogLevel.Trace, 11500,
			"Transporting response {@Response} for request {RelayRequestId} on connection {TransportConnectionId}");

	/// <summary>
	/// Initializes a new instance of the <see cref="ResponseTransport{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="hubConnection">The <see cref="HubConnection"/>.</param>
	public ResponseTransport(ILogger<ResponseTransport<T>> logger, HubConnection hubConnection)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_hubConnection = hubConnection ?? throw new ArgumentNullException(nameof(hubConnection));
	}

	/// <inheritdoc />
	public async Task TransportAsync(T response, CancellationToken cancellationToken = default)
	{
		_logTransportingResponse(_logger, response, response.RequestId, _hubConnection.ConnectionId, null);

		try
		{
			await _hubConnection.InvokeAsync("Deliver", response, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(11501, ex,
				"An error occured while transporting response for request {RelayRequestId} on connection {TransportConnectionId}",
				response.RequestId, _hubConnection.ConnectionId);
		}
	}
}
