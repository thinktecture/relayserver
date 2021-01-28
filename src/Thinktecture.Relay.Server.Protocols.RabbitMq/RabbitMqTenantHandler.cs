using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantHandler{TRequest}" />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class RabbitMqTenantHandler<TRequest, TResponse> : ITenantHandler<TRequest>, IAsyncDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RabbitMqTenantHandler<TRequest, TResponse>> _logger;
		private readonly IAcknowledgeCoordinator _acknowledgeCoordinator;
		private readonly Guid _originId;
		private readonly string _connectionId;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _consumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TRequest>? RequestReceived;

		/// <summary>
		/// Initializes a new instance of the <see cref="RabbitMqTenantHandler{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		public RabbitMqTenantHandler(ILogger<RabbitMqTenantHandler<TRequest, TResponse>> logger, RelayServerContext relayServerContext,
			IAcknowledgeCoordinator acknowledgeCoordinator, ModelFactory modelFactory, Guid tenantId, string connectionId)
		{
			if (relayServerContext == null) throw new ArgumentNullException(nameof(relayServerContext));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
			_originId = relayServerContext.OriginId;
			_connectionId = connectionId;

			_model = modelFactory?.Create($"tenant handler for {tenantId} of connection {connectionId}") ??
				throw new ArgumentNullException(nameof(modelFactory));

			_consumer = _model.ConsumeQueue($"{Constants.RequestQueuePrefix}{tenantId}", autoDelete: false, autoAck: false);
			_consumer.Received += OnRequestReceived;
		}

		private async Task OnRequestReceived(object sender, BasicDeliverEventArgs @event)
		{
			var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span);
			_logger.LogTrace("Received request {RequestId} from queue {QueueName} by consumer {ConsumerTag}", request.RequestId,
				@event.RoutingKey, @event.ConsumerTag);

			if (request.AcknowledgeMode != AcknowledgeMode.Disabled)
			{
				request.AcknowledgeOriginId = _originId;
				_acknowledgeCoordinator.RegisterRequest(request.RequestId, _connectionId, @event.DeliveryTag.ToString(),
					request.IsBodyContentOutsourced());
			}
			else
			{
				await _model.AcknowledgeAsync(@event.DeliveryTag);
			}

			await RequestReceived.InvokeAsync(sender, request);
		}

		/// <inheritdoc />
		public async Task AcknowledgeAsync(string acknowledgeId)
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
		public async ValueTask DisposeAsync()
		{
			_consumer.Received -= OnRequestReceived;

			await _model.CancelConsumerTagsAsync(_consumer.ConsumerTags);
			_model.Dispose();
		}
	}
}
