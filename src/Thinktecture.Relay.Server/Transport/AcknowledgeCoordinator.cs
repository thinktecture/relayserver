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
	private readonly ILogger _logger;

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

	/// <inheritdoc />
	public void RegisterRequest(Guid requestId, string connectionId, string acknowledgeId,
		bool outsourcedRequestBodyContent)
	{
		if (_requests.ContainsKey(requestId))
		{
			Log.ReRegisterAcknowledgeState(_logger, requestId, connectionId, acknowledgeId);
		}
		else
		{
			Log.RegisterAcknowledgeState(_logger, requestId, connectionId, acknowledgeId);
		}

		_requests[requestId] = new AcknowledgeState(connectionId, acknowledgeId, outsourcedRequestBodyContent);
	}

	/// <inheritdoc />
	public async Task ProcessAcknowledgeAsync(TAcknowledge request, CancellationToken cancellationToken = default)
	{
		if (!_requests.TryRemove(request.RequestId, out var acknowledgeState))
		{
			Log.UnknownRequest(_logger, request.RequestId);
			return;
		}

		if (acknowledgeState.AcknowledgeId is not null)
		{
			await _connectorRegistry.AcknowledgeRequestAsync(acknowledgeState.ConnectionId, acknowledgeState.AcknowledgeId,
				cancellationToken);
		}
		else
		{
			Log.PrunedRequest(_logger, request.RequestId);
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
