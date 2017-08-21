using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NLog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	public class RabbitMqMessageDispatcher : IMessageDispatcher, IDisposable
	{
		private static readonly int _queueExpiration = (int)TimeSpan.FromSeconds(10).TotalMilliseconds;

		private const string _EXCHANGE_NAME = "Relay Server";

		private readonly ILogger _logger;
		private readonly IModel _model;
		private readonly UTF8Encoding _encoding;

		public RabbitMqMessageDispatcher(ILogger logger, IConnection connection)
		{
			_logger = logger;
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			_model = connection.CreateModel();
			_encoding = new UTF8Encoding(false, true);

			DeclareExchange(_EXCHANGE_NAME);
		}

		public IObservable<IOnPremiseTargetRequest> OnRequestReceived(string onPremiseId, string connectionId, bool noAck)
		{
			return Observable.Create<IOnPremiseTargetRequest>(observer =>
			{
				var queueName = "Request " + onPremiseId;
				DeclareQueue(queueName);
				_model.QueueBind(queueName, _EXCHANGE_NAME, onPremiseId);

				_logger.Debug("Creating request consumer. OnPremiseId: {0}, ConnectionId: {1}, supportsAck: {2}", onPremiseId, connectionId, !noAck);
				var consumer = new EventingBasicConsumer(_model);

				var consumerTag = _model.BasicConsume(queueName, noAck, consumer);

				EventHandler<BasicDeliverEventArgs> onReceived = (sender, args) =>
				{
					try
					{
						var json = _encoding.GetString(args.Body);
						var request = JsonConvert.DeserializeObject<OnPremiseConnectorRequest>(json);
						request.AcknowledgeId = args.DeliveryTag.ToString();

						observer.OnNext(request);
					}
					catch (Exception ex)
					{
						_logger.Error(ex, "Error during reception of an request via RabbitMQ.");
						if (!noAck)
						{
							_model.BasicAck(args.DeliveryTag, false);
						}
					}
				};
				consumer.Received += onReceived;

				return new DelegatingDisposable(_logger, () =>
				{
					_logger.Debug("Disposing request consumer. OnPremiseId: {0}, ConnectionId: {1}", onPremiseId, connectionId);
					consumer.Received -= onReceived;
					_model.BasicCancel(consumerTag);
				});
			});
		}

		public IObservable<IOnPremiseTargetResponse> OnResponseReceived(string originId)
		{
			return Observable.Create<IOnPremiseTargetResponse>(observer =>
			{
				var queueName = "Response " + originId;
				DeclareQueue(queueName);
				_model.QueueBind(queueName, _EXCHANGE_NAME, originId);

				_logger.Debug("Creating response consumer. OriginId: {0}", originId);
				var consumer = new EventingBasicConsumer(_model);
				var consumerTag = _model.BasicConsume(queueName, true, consumer);

				EventHandler<BasicDeliverEventArgs> onReceived = (sender, args) =>
				{
					try
					{
						var json = _encoding.GetString(args.Body);
						var request = JsonConvert.DeserializeObject<OnPremiseTargetResponse>(json);

						observer.OnNext(request);
					}
					catch (Exception ex)
					{
						_logger.Error(ex, "Error during reception of an request via RabbitMQ.");
					}
				};
				consumer.Received += onReceived;

				return new DelegatingDisposable(_logger, () =>
				{
					_logger.Debug("Disposing response consumer. OriginId: {0}", originId);
					consumer.Received -= onReceived;
					_model.BasicCancel(consumerTag);
				});
			});
		}

		public void AcknowledgeRequest(string onPremiseId, string acknowledgeId)
		{
			ulong deliveryTag;
			if (UInt64.TryParse(acknowledgeId, out deliveryTag))
				_model.BasicAck(deliveryTag, false);
		}

		public Task DispatchRequest(string onPremiseId, IOnPremiseTargetRequest request)
		{
			var content = _encoding.GetBytes(JsonConvert.SerializeObject(request));
			var props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2
			};
			_model.BasicPublish(_EXCHANGE_NAME, onPremiseId, false, props, content);

			return Task.CompletedTask;
		}

		public Task DispatchResponse(string originId, IOnPremiseTargetResponse response)
		{
			var content = _encoding.GetBytes(JsonConvert.SerializeObject(response));
			var props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2
			};
			_model.BasicPublish(_EXCHANGE_NAME, originId, false, props, content);

			return Task.CompletedTask;
		}

		private void DeclareExchange(string name)
		{
			_logger.Debug("Declaring exchange. Name {0}, Type: {1}", name, ExchangeType.Direct);
			_model.ExchangeDeclare(name, ExchangeType.Direct);
		}

		private void DeclareQueue(string name)
		{
			_logger.Debug("Declaring queue. Name {0}, Expiration: {1} sec", name, _queueExpiration / 1000);
			_model.QueueDeclare(name, true, false, false, new Dictionary<string, object>() { { "x-expires", _queueExpiration } });
		}

		public void Dispose()
		{
			_model?.Dispose();
		}
	}
}
