using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <inheritdoc cref="IAcknowledgeCoordinator{T}"/>
public partial class AcknowledgeCoordinator<TRequest, TAcknowledge> : IAcknowledgeCoordinator<TAcknowledge>
	where TRequest : IClientRequest
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IBodyStore _bodyStore;
	private readonly ConnectorRegistry<TRequest> _connectorRegistry;
	private readonly ILogger<AcknowledgeCoordinator<TRequest, TAcknowledge>> _logger;

	private readonly ConcurrentDictionary<Guid, AcknowledgeState> _requests =
		new ConcurrentDictionary<Guid, AcknowledgeState>();

	/// <summary>
	/// Initializes a new instance of the <see cref="AcknowledgeCoordinator{TRequest,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="bodyStore">An <see cref="IBodyStore"/>.</param>
	/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
	public AcknowledgeCoordinator(ILogger<AcknowledgeCoordinator<TRequest, TAcknowledge>> logger, IBodyStore bodyStore,
		ConnectorRegistry<TRequest> connectorRegistry)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_bodyStore = bodyStore ?? throw new ArgumentNullException(nameof(bodyStore));
		_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
	}

	[LoggerMessage(20800, LogLevel.Trace,
		"Registering acknowledge state of request {RelayRequestId} from connection {TransportConnectionId} for id {AcknowledgeId}")]
	partial void LogRegisterAcknowledgeState(Guid relayRequestId, string transportConnectionId, string acknowledgeId);

	[LoggerMessage(20802, LogLevel.Warning,
		"Re-registering an already existing request {RelayRequestId} from connection {TransportConnectionId} for id {AcknowledgeId} – this can happen when a request got queued again and will lead to a re-execution of the request but only the first result be acknowledged and the others will be discarded")]
	partial void LogReRegisterAcknowledgeState(Guid relayRequestId, string transportConnectionId, string acknowledgeId);

	/// <inheritdoc />
	public void RegisterRequest(Guid requestId, string connectionId, string acknowledgeId,
		bool outsourcedRequestBodyContent)
	{
		if (_requests.ContainsKey(requestId))
		{
			LogReRegisterAcknowledgeState(requestId, connectionId, acknowledgeId);
		}
		else
		{
			LogRegisterAcknowledgeState(requestId, connectionId, acknowledgeId);
		}

		_requests[requestId] = new AcknowledgeState(connectionId, acknowledgeId, outsourcedRequestBodyContent);
	}

	[LoggerMessage(20801, LogLevel.Warning, "Unknown request {RelayRequestId} to acknowledge received")]
	partial void LogUnknownRequest(Guid relayRequestId);

	[LoggerMessage(20803, LogLevel.Debug, "Request {RelayRequestId} was already pruned and will not be acknowledged – this happens after an auto-recovery of the message queue transport")]
	partial void LogPrunedRequest(Guid relayRequestId);

	/// <inheritdoc />
	public async Task ProcessAcknowledgeAsync(TAcknowledge request, CancellationToken cancellationToken = default)
	{
		if (!_requests.TryRemove(request.RequestId, out var acknowledgeState))
		{
			LogUnknownRequest(request.RequestId);
			return;
		}

		if (acknowledgeState.AcknowledgeId != null)
		{
			await _connectorRegistry.AcknowledgeRequestAsync(acknowledgeState.ConnectionId, acknowledgeState.AcknowledgeId,
				cancellationToken);
		}
		else
		{
			LogPrunedRequest(request.RequestId);
		}

		if (acknowledgeState.OutsourcedRequestBodyContent && request.RemoveRequestBodyContent)
		{
			await _bodyStore.RemoveRequestBodyAsync(request.RequestId, cancellationToken);
		}
	}

	/// <inheritdoc />
	public void PruneOutstandingAcknowledgeIds()
	{
		foreach (var acknowledgeState in _requests.Values)
		{
			acknowledgeState.AcknowledgeId = null;
		}
	}

	private class AcknowledgeState
	{
		public string ConnectionId { get; }

		public string? AcknowledgeId { get; set; }

		public bool OutsourcedRequestBodyContent { get; }

		public AcknowledgeState(string connectionId, string acknowledgeId, bool outsourcedRequestBodyContent)
		{
			ConnectionId = connectionId;
			AcknowledgeId = acknowledgeId;
			OutsourcedRequestBodyContent = outsourcedRequestBodyContent;
		}
	}
}
