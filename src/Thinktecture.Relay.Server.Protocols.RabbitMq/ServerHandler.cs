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
	public class ServerHandler<TResponse> : IServerHandler<TResponse>, IDisposable
		where TResponse : ITargetResponse
	{
		private readonly ILogger<ServerHandler<TResponse>> _logger;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _responseConsumer;
		private readonly AsyncEventingBasicConsumer _acknowledgeConsumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TResponse> ResponseReceived;

		/// <inheritdoc />
		public event AsyncEventHandler<IAcknowledgeRequest> AcknowledgeReceived;

		/// <summary>
		/// Initializes a new instance of <see cref="ServerHandler{TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public ServerHandler(ILogger<ServerHandler<TResponse>> logger, ModelFactory modelFactory, RelayServerContext relayServerContext)
		{
			if (relayServerContext == null) throw new ArgumentNullException(nameof(relayServerContext));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_model = modelFactory?.Create() ?? throw new ArgumentNullException(nameof(modelFactory));

			_responseConsumer = _model.ConsumeQueue($"{Constants.ResponseQueuePrefix}{relayServerContext.OriginId}");
			_responseConsumer.Received += OnResponseReceived;

			_acknowledgeConsumer = _model.ConsumeQueue($"{Constants.AcknowledgeQueuePrefix}{relayServerContext.OriginId}");
			_acknowledgeConsumer.Received += OnAcknowledgeReceived;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_responseConsumer.Received -= OnResponseReceived;
			_acknowledgeConsumer.Received -= OnAcknowledgeReceived;

			_model.CancelConsumerTags(_responseConsumer.ConsumerTags);
			_model.CancelConsumerTags(_acknowledgeConsumer.ConsumerTags);

			_model.Dispose();
		}

		private async Task OnResponseReceived(object sender, BasicDeliverEventArgs @event)
		{
			var response = JsonSerializer.Deserialize<TResponse>(@event.Body.Span);
			_logger.LogTrace("Received response {@Response} from queue {QueueName} by consumer {ConsumerTag}", response, @event.RoutingKey,
				@event.ConsumerTag);
			await ResponseReceived.InvokeAsync(sender, response);
		}

		private async Task OnAcknowledgeReceived(object sender, BasicDeliverEventArgs @event)
		{
			var response = JsonSerializer.Deserialize<IAcknowledgeRequest>(@event.Body.Span);
			_logger.LogTrace("Received acknowledge request {@AcknowledgeRequest} from queue {QueueName} by consumer {ConsumerTag}", response,
				@event.RoutingKey, @event.ConsumerTag);
			await AcknowledgeReceived.InvokeAsync(sender, response);
		}
	}
}
