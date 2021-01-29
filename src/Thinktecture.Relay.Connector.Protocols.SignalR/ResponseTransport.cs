using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR
{
	/// <inheritdoc />
	public class ResponseTransport<T> : IResponseTransport<T>
		where T : ITargetResponse
	{
		private readonly ILogger<ResponseTransport<T>> _logger;
		private readonly HubConnection _hubConnection;

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
			_logger.LogTrace("Transporting response {@Response} for request {RequestId} on connection {ConnectionId}", response,
				response.RequestId, _hubConnection.ConnectionId);

			try
			{
				await _hubConnection.InvokeAsync("Deliver", response, cancellationToken);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "An error occured while transporting response for request {RequestId} on connection {ConnectionId}",
					response.RequestId, _hubConnection.ConnectionId);
			}
		}
	}
}
