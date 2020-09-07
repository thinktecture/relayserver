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
	/// <inheritdoc cref="ITenantHandler{TRequest}" />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class TenantHandler<TRequest, TResponse> : ITenantHandler<TRequest>, IDisposable
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly RelayServerContext _relayServerContext;
		private readonly ILogger<TenantHandler<TRequest, TResponse>> _logger;
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _consumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TRequest> RequestReceived;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="serverHandler">An <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public TenantHandler(ILogger<TenantHandler<TRequest, TResponse>> logger, Guid tenantId, IServerHandler<TResponse> serverHandler,
			ModelFactory modelFactory, RelayServerContext relayServerContext)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));

			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_model = modelFactory.Create();

			serverHandler.AcknowledgeReceived += OnAcknowledgeReceived;

			_consumer = _model.ConsumeQueue($"{Constants.RequestQueuePrefix}{tenantId}", autoDelete: false, autoAck: false);
			_consumer.Received += OnRequestReceived;

			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_serverHandler.AcknowledgeReceived -= OnAcknowledgeReceived;
			_consumer.Received -= OnRequestReceived;

			_model.CancelConsumerTags(_consumer.ConsumerTags);
			_model.Dispose();
		}

		private Task OnAcknowledgeReceived(object sender, IAcknowledgeRequest request)
		{
			if (ulong.TryParse(request.AcknowledgeId, out var deliveryTag))
			{
				_logger.LogDebug("Acknowledging message {AcknowledgeId}", request.AcknowledgeId);
				_model.BasicAck(deliveryTag, false);
			}

			return Task.CompletedTask;
		}

		private async Task OnRequestReceived(object sender, BasicDeliverEventArgs @event)
		{
			var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span);
			_logger.LogTrace("Received request {@Request} from queue", request);

			if (request.AcknowledgeMode != AcknowledgeMode.Disabled)
			{
				request.AcknowledgeOriginId = _relayServerContext.OriginId;
				request.AcknowledgeId = @event.DeliveryTag.ToString();
			}
			else
			{
				_model.BasicAck(@event.DeliveryTag, false);
			}

			await RequestReceived.InvokeAsync(sender, request);
		}
	}
}
