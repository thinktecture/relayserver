using System;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantHandler{TRequest}" />
	public class TenantHandler<TRequest, TResponse> : ITenantHandler<TRequest>, IDisposable
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly RelayServerContext _relayServerContext;
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly IModel _model;
		private readonly AsyncEventingBasicConsumer _consumer;

		/// <inheritdoc />
		public event AsyncEventHandler<TRequest> RequestReceived;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantHandler{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="serverHandler">An <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public TenantHandler(Guid tenantId, IServerHandler<TResponse> serverHandler, ModelFactory modelFactory,
			RelayServerContext relayServerContext)
		{
			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));

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
				_model.BasicAck(deliveryTag, false);
			}

			return Task.CompletedTask;
		}

		private async Task OnRequestReceived(object sender, BasicDeliverEventArgs @event)
		{
			var request = JsonSerializer.Deserialize<TRequest>(@event.Body.Span);

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
