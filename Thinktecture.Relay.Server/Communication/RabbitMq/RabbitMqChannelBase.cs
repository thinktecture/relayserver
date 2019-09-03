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
		private readonly string _queuePrefix;

		protected readonly Encoding Encoding = new UTF8Encoding(false, true);
		protected readonly ILogger Logger;

		protected IModel Model { get; private set; }
		protected string Exchange { get; private set; }
		protected string ChannelId { get; private set; }
		protected IConfiguration Configuration { get; private set; }

		protected string QueueName => $"{_queuePrefix} {ChannelId}";

		protected RabbitMqChannelBase(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
		{
			Logger = logger ?? throw new ArgumentNullException(nameof(logger));
			Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			Exchange = exchange ?? throw new ArgumentNullException(nameof(exchange));
			ChannelId = channelId ?? throw new ArgumentNullException(nameof(channelId));
			_queuePrefix = queuePrefix ?? throw new ArgumentNullException(nameof(queuePrefix));

			if (connection == null) throw new ArgumentNullException(nameof(connection));

			Model = connection.CreateModel();
			DeclareExchange();
		}

		public abstract IObservable<TMessage> OnReceived();
		public abstract Task Dispatch(TMessage message);

		protected void DeclareExchange()
		{
			Logger.Verbose("Declaring exchange. exchange-name={ExchangeName}, exchange-type={ExchangeType}", Exchange, ExchangeType.Direct);
			Model.ExchangeDeclare(Exchange, ExchangeType.Direct);
		}

		protected void DeclareAndBind()
		{
			Dictionary<string, object> arguments = null;
			if (Configuration.QueueExpiration == TimeSpan.Zero)
			{
				Logger?.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}", Exchange, QueueName);
			}
			else
			{
				Logger?.Verbose("Declaring queue. exchange-name={ExchangeName}, queue-name={QueueName}, expiration={QueueExpiration}", Exchange, QueueName, Configuration.QueueExpiration);
				arguments = new Dictionary<string, object>() { ["x-expires"] = (int)Configuration.QueueExpiration.TotalMilliseconds };
			}

			try
			{
				Model.QueueDeclare(QueueName, true, false, false, arguments);
				Model.QueueBind(QueueName, Exchange, ChannelId);
			}
			catch (Exception ex)
			{
				Logger?.Error(ex, "Declaring queue failed - possible expiration change. exchange-name={ExchangeName}, queue-name={QueueName}", Exchange, QueueName);
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


		protected void Unbind()
		{
			Logger.Verbose("Unbinding queue. exchange-name={ExchangeName}, queue-name={QueueName}", Exchange, QueueName);

			Model.QueueUnbind(QueueName, Exchange, ChannelId);
		}

		protected IObservable<TMessage> CreateObservable<TMessageType>(bool autoAck = true, Action<TMessageType, ulong> callback = null)
			where TMessageType: TMessage
		{
			return Observable.Create<TMessage>(observer =>
			{
				Logger?.Debug("Creating consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

				lock (Model)
				{
					DeclareAndBind();
					var consumer = new EventingBasicConsumer(Model);
					var consumerTag = Model.BasicConsume(QueueName, autoAck, consumer);

					void OnReceived(object sender, BasicDeliverEventArgs args)
					{
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
								lock (Model)
								{
									Model.BasicAck(args.DeliveryTag, false);
								}
							}
						}
					}

					consumer.Received += OnReceived;

					return new DelegatingDisposable(Logger, () =>
					{
						Logger?.Debug("Disposing consumer. exchange-name={ExchangeName}, queue-name={QueueName}, channel-id={ChannelId}", Exchange, QueueName, ChannelId);

						consumer.Received -= OnReceived;

						lock (Model)
						{
							Model.BasicCancel(consumerTag);
							Model.BasicRecover(true);
							Model.QueueUnbind(QueueName, Exchange, null);
						}
					});
				}
			});
		}

		public void Dispose()
		{
			if (!_disposed)
			{
				if (Model != null)
				{
					Unbind();
					Model.Dispose();
					Model = null;
				}

				_disposed = true;
			}
		}
	}
}
