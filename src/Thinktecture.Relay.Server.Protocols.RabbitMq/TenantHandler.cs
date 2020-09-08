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
	public class TenantHandler<TRequest, TResponse> : ITenantHandler<TRequest>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly Guid _originId;
		private readonly ILogger<TenantHandler<TRequest, TResponse>> _logger;
		private readonly string _connectionId;
		private readonly IAcknowledgeCoordinator _acknowledgeCoordinator;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _consumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TRequest> RequestReceived;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator"/>.</param>
		public TenantHandler(ILogger<TenantHandler<TRequest, TResponse>> logger, Guid tenantId, string connectionId,
			ModelFactory modelFactory, RelayServerContext relayServerContext,
			IAcknowledgeCoordinator acknowledgeCoordinator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_connectionId = connectionId;
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));

			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			if (relayServerContext == null)
			{
				throw new ArgumentNullException(nameof(relayServerContext));
			}

			_originId = relayServerContext.OriginId;

			_model = modelFactory.Create();
			_consumer = _model.ConsumeQueue($"{Constants.RequestQueuePrefix}{tenantId}", autoDelete: false, autoAck: false);
			_consumer.Received += OnRequestReceived;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_consumer.Received -= OnRequestReceived;

			_model.CancelConsumerTags(_consumer.ConsumerTags);
			_model.Dispose();
		}

		private async Task OnRequestReceived(object sender, BasicDeliverEventArgs @event)
		{
			var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span);
			_logger.LogTrace("Received request {@Request} from queue {QueueName} by consumer {ConsumerTag}", request, @event.RoutingKey,
				@event.ConsumerTag);

			if (request.AcknowledgeMode != AcknowledgeMode.Disabled)
			{
				request.AcknowledgeOriginId = _originId;
				_acknowledgeCoordinator.RegisterRequest(request.RequestId, _connectionId, @event.DeliveryTag.ToString(),
					request.IsBodyContentOutsourced());
			}
			else
			{
				_model.BasicAck(@event.DeliveryTag, false);
			}

			await RequestReceived.InvokeAsync(sender, request);
		}

		/// <inheritdoc />
		public Task AcknowledgeAsync(string acknowledgeId)
		{
			if (ulong.TryParse(acknowledgeId, out var deliveryTag))
			{
				_logger.LogDebug("Acknowledging {AcknowledgeId}", acknowledgeId);
				_model.BasicAck(deliveryTag, false);
			}
			else
			{
				_logger.LogWarning("Could not parse acknowledge id {AcknowledgeId}", acknowledgeId);
			}

			return Task.CompletedTask;
		}
	}
}
