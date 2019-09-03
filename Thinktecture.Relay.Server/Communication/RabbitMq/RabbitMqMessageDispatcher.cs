using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reactive.Linq;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	public class RabbitMqMessageDispatcher : IMessageDispatcher, IDisposable
	{
		private const string _EXCHANGE_NAME = "RelayServer";
		private const string _REQUEST_QUEUE_PREFIX = "Request ";
		private const string _RESPONSE_QUEUE_PREFIX = "Response ";
		private const string _ACKNOWLEDGE_QUEUE_PREFIX = "Acknowledge ";

		private readonly ILogger _logger;
		private readonly IConfiguration _configuration;
		private readonly IModel _model;
		private readonly UTF8Encoding _encoding;
		private readonly Guid _originId;

		public RabbitMqMessageDispatcher(ILogger logger, IConfiguration configuration, IConnection connection, IPersistedSettings persistedSettings)
		{
			_logger = logger;
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));

			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			_model = connection.CreateModel();
			_encoding = new UTF8Encoding(false, true);

			_originId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));

			DeclareExchange(_EXCHANGE_NAME);
		}

		public IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool autoAck)
		{
			return CreateConsumerObservable<OnPremiseConnectorRequest>($"{_REQUEST_QUEUE_PREFIX}{linkId}", autoAck, (request, deliveryTag) =>
			{
				if (autoAck)
				{
					return;
				}

				switch (request.AcknowledgmentMode)
				{
					case AcknowledgmentMode.Auto:
						lock (_model)
						{
							_model.BasicAck(deliveryTag, false);
						}

						_logger?.Debug("Request was automatically acknowledged. request-id={RequestId}", request.RequestId);
						break;

					case AcknowledgmentMode.Default:
					case AcknowledgmentMode.Manual:
						request.AcknowledgeId = deliveryTag.ToString();
						request.AcknowledgeOriginId = _originId;
						_logger?.Verbose("Request acknowledge id was set. request-id={RequestId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeId);
						break;
				}
			});
		}

		public IObservable<IOnPremiseConnectorResponse> OnResponseReceived()
		{
			return CreateConsumerObservable<OnPremiseConnectorResponse>($"{_RESPONSE_QUEUE_PREFIX}{_originId}");
		}

		public IObservable<string> OnAcknowledgeReceived()
		{
			return CreateConsumerObservable<string>($"{_ACKNOWLEDGE_QUEUE_PREFIX}{_originId}");
		}

		private IObservable<T> CreateConsumerObservable<T>(string queueName, bool autoAck = true, Action<T, ulong> callback = null)
		{
			return Observable.Create<T>(observer =>
			{
				lock (_model)
				{
					DeclareQueue(queueName);
					_model.QueueBind(queueName, _EXCHANGE_NAME, queueName);
				}

				_logger?.Debug("Creating consumer. queue-name={QueueName}", queueName);

				var consumer = new EventingBasicConsumer(_model);

				string consumerTag;
				lock (_model)
				{
					consumerTag = _model.BasicConsume(queueName, autoAck, consumer);
				}

				void OnReceived(object sender, BasicDeliverEventArgs args)
				{
					try
					{
						var json = _encoding.GetString(args.Body);
						var message = JsonConvert.DeserializeObject<T>(json);

						callback?.Invoke(message, args.DeliveryTag);

						observer.OnNext(message);
					}
					catch (Exception ex)
					{
						_logger?.Error(ex, "Error during receiving a message via RabbitMQ");

						if (!autoAck)
						{
							lock (_model)
							{
								_model.BasicAck(args.DeliveryTag, false);
							}
						}
					}
				}

				consumer.Received += OnReceived;

				return new DelegatingDisposable(_logger, () =>
				{
					_logger?.Debug("Disposing consumer. queue-name={QueueName}", queueName);

					consumer.Received -= OnReceived;

					lock (_model)
					{
						_model.BasicCancel(consumerTag);
						_model.QueueUnbind(queueName, _EXCHANGE_NAME, null);
					}
				});
			});
		}

		public void AcknowledgeRequest(string acknowledgeId)
		{
			if (UInt64.TryParse(acknowledgeId, out var deliveryTag))
			{
				_logger?.Debug("Acknowledging request. acknowledge-id={AcknowledgeId}", acknowledgeId);

				lock (_model)
				{
					_model.BasicAck(deliveryTag, false);
				}
			}
		}

		public void DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			var content = Serialize(request, out var props);

			if (request.Expiration != TimeSpan.Zero)
			{
				_logger?.Verbose("Setting RabbitMQ message TTL. request-id={RequestId}, request-expiration={RequestExpiration}", request.RequestId, request.Expiration);
				props.Expiration = request.Expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
			}

			lock (_model)
			{
				_model.BasicPublish(_EXCHANGE_NAME, $"{_REQUEST_QUEUE_PREFIX}{linkId}", false, props, content);
			}
		}

		public void DispatchResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			var content = Serialize(response, out var props);

			lock (_model)
			{
				_model.BasicPublish(_EXCHANGE_NAME, $"{_RESPONSE_QUEUE_PREFIX}{originId}", false, props, content);
			}
		}

		public void DispatchAcknowledge(Guid originId, string acknowledgeId)
		{
			var content = Serialize(acknowledgeId, out var props);

			lock (_model)
			{
				_model.BasicPublish(_EXCHANGE_NAME, $"{_ACKNOWLEDGE_QUEUE_PREFIX}{originId}", false, props, content);
			}
		}

		private byte[] Serialize<T>(T message, out BasicProperties props)
		{
			props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2,
			};

			return _encoding.GetBytes(JsonConvert.SerializeObject(message));
		}

		private void DeclareExchange(string name)
		{
			_logger?.Verbose("Declaring exchange. name={ExchangeName}, type={ExchangeType}", name, ExchangeType.Direct);

			lock (_model)
			{
				_model.ExchangeDeclare(name, ExchangeType.Direct);
			}
		}

		private void DeclareQueue(string name)
		{
			Dictionary<string, object> arguments = null;
			if (_configuration.QueueExpiration == TimeSpan.Zero)
			{
				_logger?.Verbose("Declaring queue. name={QueueName}", name);
			}
			else
			{
				_logger?.Verbose("Declaring queue. name={QueueName}, expiration={QueueExpiration}", name, _configuration.QueueExpiration);
				arguments = new Dictionary<string, object>() { ["x-expires"] = (int)_configuration.QueueExpiration.TotalMilliseconds };
			}

			try
			{
				lock (_model)
				{
					_model.QueueDeclare(name, true, false, false, arguments);
				}
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Declaring queue failed - possible expiration change. name={QueueName}", name);
				throw;
			}
		}

		private void Dispose(bool disposing)
		{
			if (disposing)
			{
				lock (_model)
				{
					_model.Dispose();
				}
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}
	}
}
