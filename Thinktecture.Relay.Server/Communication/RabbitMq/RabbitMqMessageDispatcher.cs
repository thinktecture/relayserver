using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
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

		public IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool noAck)
		{
			return Observable.Create<IOnPremiseConnectorRequest>(observer =>
			{
				var queueName = "Request " + linkId;

				DeclareQueue(queueName);
				_model.QueueBind(queueName, _EXCHANGE_NAME, linkId.ToString());

				_logger?.Verbose("Creating request consumer. link-id={LinkId}, connection-id={ConnectionId}, supports-ack={ConnectionSupportsAck}", linkId, connectionId, !noAck);

				var consumer = new EventingBasicConsumer(_model);

				var consumerTag = _model.BasicConsume(queueName, noAck, consumer);

				void OnReceived(object sender, BasicDeliverEventArgs args)
				{
					try
					{
						var json = _encoding.GetString(args.Body);
						var request = JsonConvert.DeserializeObject<OnPremiseConnectorRequest>(json);

						if (!noAck)
						{
							if (request.AutoAcknowledge)
							{
								_model.BasicAck(args.DeliveryTag, false);
								_logger?.Debug("Request was auto-acknowledged. request-id={RequestId}", request.RequestId);
							}
							else
							{
								request.AcknowledgeId = args.DeliveryTag.ToString();
								_logger?.Verbose("Request acknowledge id was set. request-id={RequestId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeId);
							}
						}
						
						observer.OnNext(request);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during reception of an request via RabbitMQ");
						if (!noAck)
						{
							_model.BasicAck(args.DeliveryTag, false);
						}
					}
				}

				consumer.Received += OnReceived;

				return new DelegatingDisposable(_logger, () =>
				{
					_logger?.Debug("Disposing request consumer. link-id={LinkId}, connection-id={ConnectionId}", linkId, connectionId);
					consumer.Received -= OnReceived;
					_model.BasicCancel(consumerTag);
				});
			});
		}

		public IObservable<IOnPremiseConnectorResponse> OnResponseReceived(Guid originId)
		{
			return Observable.Create<IOnPremiseConnectorResponse>(observer =>
			{
				var queueName = "Response " + originId;
				DeclareQueue(queueName);
				_model.QueueBind(queueName, _EXCHANGE_NAME, originId.ToString());

				_logger?.Debug("Creating response consumer");

				var consumer = new EventingBasicConsumer(_model);
				var consumerTag = _model.BasicConsume(queueName, true, consumer);

				void OnReceived(object sender, BasicDeliverEventArgs args)
				{
					try
					{
						var json = _encoding.GetString(args.Body);
						var request = JsonConvert.DeserializeObject<OnPremiseConnectorResponse>(json);

						observer.OnNext(request);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during reception of an request via RabbitMQ");
					}
				}

				consumer.Received += OnReceived;

				return new DelegatingDisposable(_logger, () =>
				{
					_logger?.Debug("Disposing response consumer");

					consumer.Received -= OnReceived;
					_model.BasicCancel(consumerTag);
				});
			});
		}

		public void AcknowledgeRequest(Guid linkId, string acknowledgeId)
		{
			if (UInt64.TryParse(acknowledgeId, out var deliveryTag))
				_model.BasicAck(deliveryTag, false);
		}

		public Task DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			var content = _encoding.GetBytes(JsonConvert.SerializeObject(request));
			var props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2
			};
			_model.BasicPublish(_EXCHANGE_NAME, linkId.ToString(), false, props, content);

			return Task.CompletedTask;
		}

		public Task DispatchResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			var content = _encoding.GetBytes(JsonConvert.SerializeObject(response));
			var props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2
			};
			_model.BasicPublish(_EXCHANGE_NAME, originId.ToString(), false, props, content);

			return Task.CompletedTask;
		}

		private void DeclareExchange(string name)
		{
			_logger?.Verbose("Declaring exchange. name={ExchangeName}, type={ExchangeType}", name, ExchangeType.Direct);
			_model.ExchangeDeclare(name, ExchangeType.Direct);
		}

		private void DeclareQueue(string name)
		{
			_logger?.Verbose("Declaring queue. name={QueueName}, expiration={QueueExpiration} sec", name, _queueExpiration / 1000);
			_model.QueueDeclare(name, true, false, false, new Dictionary<string, object>() { ["x-expires"] = _queueExpiration });
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				_model?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
