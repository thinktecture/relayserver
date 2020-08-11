using System;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="IServerHandler{TResponse}" />
	public class ServerHandler<TResponse> : IServerHandler<TResponse>, IDisposable
		where TResponse : IRelayTargetResponse
	{
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
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public ServerHandler(ModelFactory modelFactory, RelayServerContext relayServerContext)
		{
			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_model = modelFactory.Create();

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
			=> await ResponseReceived.InvokeAsync(sender, JsonSerializer.Deserialize<TResponse>(@event.Body.Span));

		private async Task OnAcknowledgeReceived(object sender, BasicDeliverEventArgs @event)
			=> await AcknowledgeReceived.InvokeAsync(sender, JsonSerializer.Deserialize<IAcknowledgeRequest>(@event.Body.Span));
	}
}
