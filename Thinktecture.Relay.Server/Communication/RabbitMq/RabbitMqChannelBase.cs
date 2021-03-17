using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal abstract class RabbitMqChannelBase<TMessage> : IRabbitMqChannel<TMessage>
		where TMessage : class
	{
		private bool _disposed = false;
		private bool _declaredAndBound = false;
		private IModel _model;

		private readonly string _queuePrefix;

		protected readonly Encoding Encoding = new UTF8Encoding(false, true);
		protected readonly ILogger Logger;

		protected string Exchange { get; private set; }
		protected string ChannelId { get; private set; }
		protected IConfiguration Configuration { get; private set; }

		protected string QueueName => $"{_queuePrefix} {ChannelId}";
		protected string RoutingKey => QueueName;

		protected RabbitMqChannelBase(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
			ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
			_queuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

			if (connection == null) throw new ArgumentNullException(nameof(connection));

			_model = connection.CreateModel();
			DeclareExchange();
		}

		public abstract IObservable<TMessage> OnReceived();

		public virtual Task Dispatch(TMessage request)
		{
			var data = Serialize(request, out var properties);
			Send(data, properties);

			return Task.CompletedTask;
		}

		protected void DeclareExchange()
		{
			Logger.Verbose("Declaring exchange. exchange-name={ExchangeName}, exchange-type={ExchangeType}", Exchange, ExchangeType.Direct);
			_model.ExchangeDeclare(Exchange, ExchangeType.Direct);
		}

		protected void DeclareAndBind()
		{
			if (_declaredAndBound)
			{
				// Nothing to do here, the model is already in a working state.
				return;
			}

			Dictionary<string, object> arguments = null;
			if (Configuration.QueueExpiration == TimeSpan.Zero)
			{
				Logger?.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
			}
			else
			{
				Logger?.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, expiration={QueueExpiration}", Exchange, QueueName, ChannelId, Configuration.QueueExpiration);
				arguments = new Dictionary<string, object>() { ["x-expires"] = (int)Configuration.QueueExpiration.TotalMilliseconds };
			}

			try
			{
				_model.QueueDeclare(QueueName, true, false, false, arguments);
				_model.QueueBind(QueueName, Exchange, RoutingKey);
				_declaredAndBound = true;
			}
			catch (Exception ex)
			{
				Logger?.Error(ex, "Declaring queue failed - possible expiration change. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
				throw;
			}
		}

		protected byte[] Serialize<T>(T message, out BasicProperties props)
		{
			props = new BasicProperties()
			{
				ContentEncoding = "application/json",
				DeliveryMode = 2, // Mode 2: persistent message
			};

			return Encoding.GetBytes(JsonConvert.SerializeObject(message));
		}

		protected void Send(byte[] data, IBasicProperties properties)
		{
			Logger.Verbose("Sending data. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, data-length={DataLength}", Exchange, QueueName, ChannelId, data.Length);

			lock (_model)
			{
				DeclareAndBind();
				_model.BasicPublish(Exchange, RoutingKey, false, properties, data);
			}
		}

		protected void Unbind()
		{
			Logger.Verbose("Unbinding queue. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

			_model.QueueUnbind(QueueName, Exchange, RoutingKey);
			_declaredAndBound = false;
		}

		protected IObservable<TMessage> CreateObservable<TMessageType>(bool autoAck = true, Action<TMessageType, ulong> callback = null)
			where TMessageType : class, TMessage
		{
			return Observable.Create<TMessage>(observer =>
			{
				Logger?.Debug("Creating consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

				lock (_model)
				{
					DeclareAndBind();
					var consumer = new EventingBasicConsumer(_model);
					var consumerTag = _model.BasicConsume(QueueName, autoAck, consumer);

					void OnReceived(object sender, BasicDeliverEventArgs args)
					{
						Logger?.Verbose("Received data. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, data-length={DataLength}", Exchange, QueueName, ChannelId, args.Body.Length);
						try
						{
							var json = Encoding.GetString(args.Body);
							var message = JsonConvert.DeserializeObject<TMessageType>(json);

							callback?.Invoke(message, args.DeliveryTag);
							observer.OnNext(message);
						}
						catch (Exception ex)
						{
							Logger?.Error(ex, "Error during receiving a message via RabbitMQ. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

							if (!autoAck)
							{
								Acknowledge(args.DeliveryTag);
							}
						}
					}

					consumer.Received += OnReceived;

					return new DelegatingDisposable(Logger, () =>
					{
						Logger?.Debug("Disposing consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

						consumer.Received -= OnReceived;

						lock (_model)
						{
							_model.BasicCancel(consumerTag);
							_model.BasicRecover(true);
							Unbind();
						}
					});
				}
			});
		}

		protected void Acknowledge(ulong deliveryTag, bool multiple = false)
		{
			lock (_model)
			{
				_model.BasicAck(deliveryTag, multiple);
			}
		}

		public void Dispose()
		{
			Logger?.Debug("Disposing channel. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

			if (!_disposed)
			{
				if (_model != null)
				{
					Unbind();
					_model.Dispose();
					_model = null;
				}

				_disposed = true;
			}
		}
	}

	internal class RabbitMqChannelBase<TMessage, TInstance> : RabbitMqChannelBase<TMessage>
		where TMessage : class
		where TInstance : class, TMessage
	{
		protected RabbitMqChannelBase(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}

		public override IObservable<TMessage> OnReceived()
		{
			return CreateObservable<TInstance>();
		}
	}
}
