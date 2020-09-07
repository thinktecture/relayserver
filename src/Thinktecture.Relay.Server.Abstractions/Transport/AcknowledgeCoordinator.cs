using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a coordinator for acknowledgements.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class AcknowledgeCoordinator<TRequest, TResponse> : IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<AcknowledgeCoordinator<TRequest, TResponse>> _logger;
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;

		private class AcknowledgeState
		{
			public DateTime Creation { get; } = DateTime.UtcNow;

			public string ConnectionId { get; set; }
			public string AcknowledgeId { get; set; }
		}

		private readonly ConcurrentDictionary<Guid, AcknowledgeState> _requests = new ConcurrentDictionary<Guid, AcknowledgeState>();

		/// <summary>
		/// Initializes a new instance of <see cref="AcknowledgeCoordinator{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serverHandler">An <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		public AcknowledgeCoordinator(ILogger<AcknowledgeCoordinator<TRequest, TResponse>> logger, IServerHandler<TResponse> serverHandler,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));

			_serverHandler.AcknowledgeReceived += OnAcknowledgeReceived;
		}

		private Task OnAcknowledgeReceived(object sender, IAcknowledgeRequest request) => AcknowledgeRequestAsync(request.RequestId);

		/// <inheritdoc />
		public void Dispose() => _serverHandler.AcknowledgeReceived -= OnAcknowledgeReceived;

		/// <summary>
		/// Registers an <see cref="AcknowledgeState"/>.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="acknowledgeId">The id to acknowledge.</param>
		public void RegisterRequest(Guid requestId, string connectionId, string acknowledgeId)
		{
			_logger.LogTrace("Registering acknowledge state of request {RequestId} from connection {ConnectionId} for id {AcknowledgeId}",
				requestId, connectionId, acknowledgeId);
			_requests[requestId] = new AcknowledgeState() { ConnectionId = connectionId, AcknowledgeId = acknowledgeId };
		}

		/// <summary>
		/// Acknowledges the request.
		/// </summary>
		/// <param name="requestId">The unique id of the request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public async Task AcknowledgeRequestAsync(Guid requestId)
		{
			if (_requests.TryRemove(requestId, out var acknowledgeState))
			{
				await _tenantConnectorAdapterRegistry.AcknowledgeRequestAsync(acknowledgeState.ConnectionId, acknowledgeState.AcknowledgeId);
				return;
			}

			_logger.LogWarning("Unknown request {RequestId} to acknowledge received", requestId);
		}
	}
}
