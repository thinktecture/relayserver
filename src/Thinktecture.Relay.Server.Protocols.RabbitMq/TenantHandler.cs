using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

/// <inheritdoc cref="ITenantHandler"/>
// ReSharper disable once ClassNeverInstantiated.Global
public partial class TenantHandler<TRequest, TAcknowledge> : ITenantHandler, IDisposable
	where TRequest : IClientRequest
	where TAcknowledge : IAcknowledgeRequest
{
	// ReSharper disable once NotAccessedField.Local; Justification: Used by LoggerMessage source generator
	private readonly ILogger<TenantHandler<TRequest, TAcknowledge>> _logger;
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly string _connectionId;
	private readonly ConnectorRegistry<TRequest> _connectorRegistry;
	private readonly AsyncEventingBasicConsumer _consumer;
	private readonly IModel _model;
	private readonly RelayServerContext _relayServerContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantHandler{TRequest,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
	/// <param name="tenantName">The unique name of the tenant.</param>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="maximumConcurrentRequests">The amount of maximum concurrent requests.</param>
	/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
	/// <param name="modelFactory">The <see cref="ModelFactory{TAcknowledge}"/>.</param>
	/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
	/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
	public TenantHandler(ILogger<TenantHandler<TRequest, TAcknowledge>> logger, string tenantName, string connectionId,
		int maximumConcurrentRequests, ConnectorRegistry<TRequest> connectorRegistry,
		ModelFactory<TAcknowledge> modelFactory, RelayServerContext relayServerContext,
		IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator)
	{
		_connectionId = connectionId;
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
		_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
		_acknowledgeCoordinator =
			acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));

		if (modelFactory is null) throw new ArgumentNullException(nameof(modelFactory));

		_model = modelFactory.Create($"tenant handler for {tenantName} of connection {connectionId}", true);

		if (maximumConcurrentRequests > ushort.MinValue && maximumConcurrentRequests <= ushort.MaxValue)
		{
			_model.BasicQos(0, (ushort)maximumConcurrentRequests, false);
		}

		_consumer = _model.ConsumeQueue(_logger, $"{Constants.RequestQueuePrefix} {tenantName}", autoDelete: false,
			autoAck: false);
		_consumer.Received += ConsumerReceived;
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_consumer.Received -= ConsumerReceived;

		_model.Dispose();
	}

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25300, LogLevel.Trace, "Acknowledging {AcknowledgeId}")]
	partial void LogAcknowledge(string acknowledgeId);

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25301, LogLevel.Warning, "Could not parse acknowledge id {AcknowledgeId}")]
	partial void LogCouldNotParseAcknowledge(string acknowledgeId);

	/// <inheritdoc />
	public async Task AcknowledgeAsync(string acknowledgeId, CancellationToken cancellationToken = default)
	{
		if (ulong.TryParse(acknowledgeId, out var deliveryTag))
		{
			LogAcknowledge(acknowledgeId);
			await _model.AcknowledgeAsync(deliveryTag);
		}
		else
		{
			LogCouldNotParseAcknowledge(acknowledgeId);
		}
	}

	// ReSharper disable once PartialMethodWithSinglePart; Justification: Source generator
	[LoggerMessage(25302, LogLevel.Trace,
		"Received request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
	partial void LogReceivedRequest(Guid relayRequestId, string queueName, string consumerTag);

	private async Task ConsumerReceived(object sender, BasicDeliverEventArgs @event)
	{
		var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span) ??
			throw new Exception("Could not deserialize request.");

		LogReceivedRequest(request.RequestId, @event.RoutingKey, @event.ConsumerTag);

		var acknowledgeId = @event.DeliveryTag.ToString();

		if (request.AcknowledgeMode == AcknowledgeMode.Disabled)
		{
			LogAcknowledge(acknowledgeId);
			await _model.AcknowledgeAsync(@event.DeliveryTag);
		}
		else
		{
			request.AcknowledgeOriginId = _relayServerContext.OriginId;
			_acknowledgeCoordinator.RegisterRequest(request.RequestId, _connectionId, acknowledgeId,
				request.IsBodyContentOutsourced());
		}

		await _connectorRegistry.TransportRequestAsync(_connectionId, request);
	}
}
