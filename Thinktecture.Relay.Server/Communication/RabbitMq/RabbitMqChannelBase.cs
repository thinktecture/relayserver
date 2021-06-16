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
		private readonly IConnection _connection;

		private class Consumer
		{
			public Func<string> CreateConsumer { get; set; }
			public string Tag { get; set; }
		}

		private readonly Dictionary<IObserver<TMessage>, Consumer> _observers = new Dictionary<IObserver<TMessage>, Consumer>();

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
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));

			CreateModel();
		}

		private void CreateModel()
		{
			_model = _connection.CreateModel();
			_model.ModelShutdown += OnModelShutdown;
			_declaredAndBound = false;

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
				Logger.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
			}
			else
			{
				Logger.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, expiration={QueueExpiration}", Exchange, QueueName, ChannelId, Configuration.QueueExpiration);
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
				Logger.Error(ex, "Declaring queue failed - possible expiration change. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
				throw;
			}
		}

		protected void OnModelShutdown(object sender, ShutdownEventArgs args)
		{
			Logger.Warning("Model shutdown detected. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, shutdown-reason={ShutdownReason}", Exchange, QueueName, ChannelId, _model.CloseReason);

			// The connection is locked in the event handler, so we can't create a new model directly in here
			Task.Delay(TimeSpan.FromSeconds(1)).ContinueWith(_ => RecreateModel());
		}

		protected void RecreateModel()
		{
			lock (this)
			{
				Logger.Information("Recreating model. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, shutdown-reason={ShutdownReason}", Exchange, QueueName, ChannelId, _model.CloseReason);

				// cleanup old model
				_model.ModelShutdown -= OnModelShutdown;
				_model.Dispose();

				// prepare new model
				CreateModel();
				DeclareAndBind();

				// re-attach the consumers
				foreach (var consumer in _observers.Values)
				{
					var oldConsumerTag = consumer.Tag;
					consumer.Tag = consumer.CreateConsumer();
					Logger.Verbose("Recreated consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, consumer-tag={ConsumerTag}, old-consumer-tag={OldConsumerTag}", Exchange, QueueName, ChannelId, consumer.Tag, oldConsumerTag);
				}
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

			lock (this)
			{
				DeclareAndBind();
				_model.BasicPublish(Exchange, RoutingKey, false, properties, data);
			}
		}

		protected IObservable<TMessage> CreateObservable<TMessageType>(bool autoAck = true, Action<TMessageType, ulong> callback = null)
			where TMessageType : class, TMessage
		{
			return Observable.Create<TMessage>(observer =>
			{
				Logger.Debug("Creating consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

				lock (this)
				{
					DeclareAndBind();

					string CreateConsumer()
					{
						var consumer = new EventingBasicConsumer(_model);

						void OnReceived(object sender, BasicDeliverEventArgs args)
						{
							Logger.Verbose("Received data. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, data-length={DataLength}", Exchange, QueueName, ChannelId, args.Body.Length);
							try
							{
								var json = Encoding.GetString(args.Body);
								var message = JsonConvert.DeserializeObject<TMessageType>(json);

								callback?.Invoke(message, args.DeliveryTag);
								observer.OnNext(message);
							}
							catch (Exception ex)
							{
								Logger.Error(ex, "Error during receiving a message via RabbitMQ. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

								if (!autoAck)
								{
									Acknowledge(args.DeliveryTag);
								}
							}
						}

						consumer.Received += OnReceived;
						_model.BasicConsume(QueueName, autoAck, consumer);

						Logger.Verbose("Created consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, consumer-tag={ConsumerTag}", Exchange, QueueName, ChannelId, consumer.ConsumerTag);

						return consumer.ConsumerTag;
					}

					var consumerTag = CreateConsumer();
					_observers[observer] = new Consumer() { CreateConsumer = CreateConsumer, Tag = consumerTag, };

					return new DelegatingDisposable(Logger, () =>
					{
						lock (this)
						{
							try
							{
								if (_observers.TryGetValue(observer, out var consumer))
								{
									Logger.Debug("Disposing consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}, consumer-tag={ConsumerTag}", Exchange, QueueName, ChannelId, consumer.Tag);

									_observers.Remove(observer);

									if (!String.IsNullOrEmpty(consumer.Tag))
									{
										_model.BasicCancel(consumer.Tag);
										_declaredAndBound = false;
										consumer.Tag = null;
									}
								}
								else
								{
									Logger.Warning("Could not dispose consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
								}
							}
							catch (Exception ex)
							{
								Logger.Error(ex, "Error while disposing consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);
							}
						}
					});
				}
			});
		}

		protected void Acknowledge(ulong deliveryTag, bool multiple = false)
		{
			lock (this)
			{
				_model.BasicAck(deliveryTag, multiple);
			}
		}

		public void Dispose()
		{
			Logger.Debug("Disposing channel. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

			if (!_disposed)
			{
				_disposed = true;

				if (_model != null)
				{
					_model.Dispose();
					_model = null;
				}
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
