using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="IServerHandler{TResponse}" />
	public class RabbitMqServerHandler<TResponse> : IServerHandler<TResponse>, IAsyncDisposable
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RabbitMqServerHandler<TResponse>> _logger;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _responseConsumer;
		private readonly AsyncEventingBasicConsumer _acknowledgeConsumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TResponse>? ResponseReceived;

		/// <inheritdoc />
		public event AsyncEventHandler<IAcknowledgeRequest>? AcknowledgeReceived;

		/// <summary>
		/// Initializes a new instance of the <see cref="RabbitMqServerHandler{TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public RabbitMqServerHandler(ILogger<RabbitMqServerHandler<TResponse>> logger, ModelFactory modelFactory,
			RelayServerContext relayServerContext)
		{
			if (modelFactory == null) throw new ArgumentNullException(nameof(modelFactory));
			if (relayServerContext == null) throw new ArgumentNullException(nameof(relayServerContext));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_model = modelFactory.Create("server handler");

			_responseConsumer = _model.ConsumeQueue($"{Constants.ResponseQueuePrefix}{relayServerContext.OriginId}");
			_responseConsumer.Received += OnResponseReceived;

			_acknowledgeConsumer = _model.ConsumeQueue($"{Constants.AcknowledgeQueuePrefix}{relayServerContext.OriginId}");
			_acknowledgeConsumer.Received += OnAcknowledgeReceived;
		}

		private async Task OnResponseReceived(object sender, BasicDeliverEventArgs @event)
		{
			var response = JsonSerializer.Deserialize<TResponse>(@event.Body.Span);
			_logger.LogTrace("Received response for {RequestId} from queue {QueueName} by consumer {ConsumerTag}", response.RequestId,
				@event.RoutingKey, @event.ConsumerTag);
			await ResponseReceived.InvokeAsync(sender, response);
		}

		private async Task OnAcknowledgeReceived(object sender, BasicDeliverEventArgs @event)
		{
			var response = JsonSerializer.Deserialize<IAcknowledgeRequest>(@event.Body.Span);
			_logger.LogTrace("Received acknowledge request {@AcknowledgeRequest} from queue {QueueName} by consumer {ConsumerTag}", response,
				@event.RoutingKey, @event.ConsumerTag);
			await AcknowledgeReceived.InvokeAsync(sender, response);
		}

		/// <inheritdoc />
		public async ValueTask DisposeAsync()
		{
			_responseConsumer.Received -= OnResponseReceived;
			_acknowledgeConsumer.Received -= OnAcknowledgeReceived;

			await _model.CancelConsumerTagsAsync(_responseConsumer.ConsumerTags);
			await _model.CancelConsumerTagsAsync(_acknowledgeConsumer.ConsumerTags);

			_model.Dispose();
		}
	}
}
