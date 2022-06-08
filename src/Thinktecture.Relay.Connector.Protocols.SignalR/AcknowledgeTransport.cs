using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Connector.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

/// <inheritdoc/>
public class AcknowledgeTransport<T> : IAcknowledgeTransport<T>
	where T : IAcknowledgeRequest
{
	private readonly HubConnection _hubConnection;
	private readonly ILogger<AcknowledgeTransport<T>> _logger;

	// Todo: Move to LoggerMessage source generator when destructuring is supported,
	// see https://github.com/dotnet/runtime/issues/69490
	private readonly Action<ILogger, IAcknowledgeRequest, Guid, string?, Exception?> _logTransportingAck =
		LoggerMessage.Define<IAcknowledgeRequest, Guid, string?>(LogLevel.Trace, 11100,
			"Transporting acknowledge request {@AcknowledgeRequest} for request {RequestId} on connection {ConnectionId}");

	/// <summary>
	/// Initializes a new instance of the <see cref="AcknowledgeTransport{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="hubConnection">The <see cref="HubConnection"/>.</param>
	public AcknowledgeTransport(ILogger<AcknowledgeTransport<T>> logger, HubConnection hubConnection)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_hubConnection = hubConnection ?? throw new ArgumentNullException(nameof(hubConnection));
	}

	/// <inheritdoc/>
	public async Task TransportAsync(T request, CancellationToken cancellationToken = default)
	{
		if (_logger.IsEnabled(LogLevel.Trace))
			_logTransportingAck(_logger, request, request.RequestId, _hubConnection.ConnectionId, null);

		try
		{
			await _hubConnection.InvokeAsync("Acknowledge", request, cancellationToken);
		}
		catch (Exception ex)
		{
			_logger.LogError(11101, ex,
				"An error occured while transporting acknowledge for request {RequestId} on connection {ConnectionId}",
				request.RequestId, _hubConnection.ConnectionId);
		}
	}
}
