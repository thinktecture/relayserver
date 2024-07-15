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
	private readonly ILogger _logger;
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly string _connectionId;
	private readonly ConnectorRegistry<TRequest> _connectorRegistry;
	private readonly DisposableConsumer _consumer;
	private readonly IModel _model;
	private readonly RelayServerContext _relayServerContext;

	/// <summary>
	/// Initializes a new instance of the <see cref="TenantHandler{TRequest,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
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

		_consumer = new DisposableConsumer(_logger, _model, $"{Constants.RequestQueuePrefix} {tenantName}",
			autoAck: false);
		_consumer.Consume(ConsumerReceivedAsync);
	}

	/// <inheritdoc />
	public void Dispose()
	{
		_consumer.Dispose();

		_model.Dispose();
	}

	/// <inheritdoc />
	public async Task AcknowledgeAsync(string acknowledgeId, CancellationToken cancellationToken = default)
	{
		if (ulong.TryParse(acknowledgeId, out var deliveryTag))
		{
			Log.Acknowledge(_logger, acknowledgeId);
			await _model.AcknowledgeAsync(deliveryTag);
		}
		else
		{
			Log.CouldNotParseAcknowledge(_logger, acknowledgeId);
		}
	}

	private async Task ConsumerReceivedAsync(BasicDeliverEventArgs @event)
	{
		var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span) ??
			throw new Exception("Could not deserialize request.");

		Log.ReceivedRequest(_logger, request.RequestId, @event.RoutingKey, @event.ConsumerTag);

		var acknowledgeId = @event.DeliveryTag.ToString();

		if (request.AcknowledgeMode == AcknowledgeMode.Disabled)
		{
			Log.Acknowledge(_logger, acknowledgeId);
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
