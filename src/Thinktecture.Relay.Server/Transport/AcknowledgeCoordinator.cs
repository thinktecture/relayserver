using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc cref="IAcknowledgeCoordinator" />
	public class AcknowledgeCoordinator<TRequest, TResponse> : IDisposable, IAcknowledgeCoordinator
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<AcknowledgeCoordinator<TRequest, TResponse>> _logger;
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;
		private readonly IBodyStore _bodyStore;
		private readonly IServerDispatcher<TResponse> _serverDispatcher;
		private readonly Guid _originId;

		private class AcknowledgeState
		{
			public AcknowledgeState(string connectionId, string acknowledgeId, bool outsourcedRequestBodyContent)
			{
				ConnectionId = connectionId;
				AcknowledgeId = acknowledgeId;
				OutsourcedRequestBodyContent = outsourcedRequestBodyContent;
			}

			public DateTime Creation { get; } = DateTime.UtcNow;

			public string ConnectionId { get; }
			public string AcknowledgeId { get; }
			public bool OutsourcedRequestBodyContent { get; }
		}

		private readonly ConcurrentDictionary<Guid, AcknowledgeState> _requests = new ConcurrentDictionary<Guid, AcknowledgeState>();

		/// <summary>
		/// Initializes a new instance of the <see cref="AcknowledgeCoordinator{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serverHandler">An <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="serverDispatcher">An <see cref="IServerDispatcher{TResponse}"/>.</param>
		public AcknowledgeCoordinator(ILogger<AcknowledgeCoordinator<TRequest, TResponse>> logger, IServerHandler<TResponse> serverHandler,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry, IBodyStore bodyStore,
			RelayServerContext relayServerContext, IServerDispatcher<TResponse> serverDispatcher)
		{
			if (relayServerContext == null) throw new ArgumentNullException(nameof(relayServerContext));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
			_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
			_serverDispatcher = serverDispatcher ?? throw new ArgumentNullException(nameof(serverDispatcher));
			_originId = relayServerContext.OriginId;

			_serverHandler.AcknowledgeReceived += OnAcknowledgeReceived;
		}

		private Task OnAcknowledgeReceived(object sender, IAcknowledgeRequest request) => AcknowledgeRequestAsync(request);

		/// <inheritdoc />
		public void Dispose() => _serverHandler.AcknowledgeReceived -= OnAcknowledgeReceived;

		/// <inheritdoc />
		public void RegisterRequest(Guid requestId, string connectionId, string acknowledgeId, bool outsourcedRequestBodyContent)
		{
			_logger.LogTrace("Registering acknowledge state of request {RequestId} from connection {ConnectionId} for id {AcknowledgeId}",
				requestId, connectionId, acknowledgeId);
			_requests[requestId] = new AcknowledgeState(connectionId, acknowledgeId, outsourcedRequestBodyContent);
		}

		/// <inheritdoc />
		public async Task AcknowledgeRequestAsync(IAcknowledgeRequest request, CancellationToken cancellationToken = default)
		{
			if (request.OriginId != _originId)
			{
				_logger.LogDebug("Redirecting acknowledgment for request {RequestId} to origin {OriginId}", request.RequestId,
					request.OriginId);
				await _serverDispatcher.DispatchAcknowledgeAsync(request);
				return;
			}

			if (!_requests.TryRemove(request.RequestId, out var acknowledgeState))
			{
				_logger.LogWarning("Unknown request {RequestId} to acknowledge received", request.RequestId);
				return;
			}

			await _tenantConnectorAdapterRegistry.AcknowledgeRequestAsync(acknowledgeState.ConnectionId, acknowledgeState.AcknowledgeId);

			if (acknowledgeState.OutsourcedRequestBodyContent && request.RemoveRequestBodyContent)
			{
				await _bodyStore.RemoveRequestBodyAsync(request.RequestId, cancellationToken);
			}
		}
	}
}
