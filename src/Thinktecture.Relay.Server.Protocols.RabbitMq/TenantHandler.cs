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

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantHandler" />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class TenantHandler<TRequest, TAcknowledge> : ITenantHandler, IDisposable
		where TRequest : IClientRequest
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly ILogger<TenantHandler<TRequest, TAcknowledge>> _logger;
		private readonly ConnectorRegistry<TRequest> _connectorRegistry;
		private readonly RelayServerContext _relayServerContext;
		private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
		private readonly string _connectionId;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _consumer;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantHandler{TRequest,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
		public TenantHandler(ILogger<TenantHandler<TRequest, TAcknowledge>> logger, Guid tenantId, string connectionId,
			ConnectorRegistry<TRequest> connectorRegistry, ModelFactory modelFactory, RelayServerContext relayServerContext,
			IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
			_connectionId = connectionId;

			_model = modelFactory?.Create($"tenant handler for {tenantId} of connection {connectionId}") ??
				throw new ArgumentNullException(nameof(modelFactory));

			_consumer = _model.ConsumeQueue($"{Constants.RequestQueuePrefix}{tenantId}", autoDelete: false, autoAck: false);
			_consumer.Received += ConsumerReceived;
		}

		private async Task ConsumerReceived(object sender, BasicDeliverEventArgs @event)
		{
			var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span);
			_logger.LogTrace("Received request {RequestId} from queue {QueueName} by consumer {ConsumerTag}", request.RequestId,
				@event.RoutingKey, @event.ConsumerTag);

			var acknowledgeId = @event.DeliveryTag.ToString();

			if (request.AcknowledgeMode == AcknowledgeMode.Disabled)
			{
				_logger.LogTrace("Acknowledging {AcknowledgeId}", acknowledgeId);
				await _model.AcknowledgeAsync(@event.DeliveryTag);
			}
			else
			{
				request.AcknowledgeOriginId = _relayServerContext.OriginId;
				_acknowledgeCoordinator.RegisterRequest(request.RequestId, _connectionId, acknowledgeId, request.IsBodyContentOutsourced());
			}

			await _connectorRegistry.TransportRequestAsync(_connectionId, request);
		}

		/// <inheritdoc />
		public async Task AcknowledgeAsync(string acknowledgeId, CancellationToken cancellationToken = default)
		{
			if (ulong.TryParse(acknowledgeId, out var deliveryTag))
			{
				_logger.LogDebug("Acknowledging {AcknowledgeId}", acknowledgeId);
				await _model.AcknowledgeAsync(deliveryTag);
			}
			else
			{
				_logger.LogWarning("Could not parse acknowledge id {AcknowledgeId}", acknowledgeId);
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_consumer.Received -= ConsumerReceived;

			_model.CancelConsumerTags(_consumer.ConsumerTags);
			_model.Dispose();
		}
	}
}
