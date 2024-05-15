using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal partial class DisposableConsumer : IDisposable
{
	private readonly ILogger _logger;
	private readonly string _queueName;
	private readonly bool _autoAck;
	private readonly bool _durable;
	private readonly bool _autoDelete;
	private readonly AsyncEventingBasicConsumer _consumer;

	private Func<BasicDeliverEventArgs, Task>? _handler;
	private string _consumerTag = string.Empty;

	/// <summary>
	/// Initializes a new instance of the <see cref="DisposableConsumer"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger"/>.</param>
	/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
	/// <param name="queueName">The name of the queue.</param>
	/// <param name="autoAck">The consumer should automatically acknowledge the message.</param>
	/// <param name="durable">The queue should survive a broker restart.</param>
	/// <param name="autoDelete">The queue should be deleted when the last consumer goes away.</param>
	public DisposableConsumer(ILogger logger, IModel model, string queueName, bool autoAck = true, bool durable = true,
		bool autoDelete = true)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));

		if (model is null) throw new ArgumentNullException(nameof(model));

		_queueName = queueName;
		_autoAck = autoAck;
		_durable = durable;
		_autoDelete = autoDelete;

		_consumer = new AsyncEventingBasicConsumer(model);
	}

	public void Consume(Func<BasicDeliverEventArgs, Task> handler)
	{
		_handler = handler;

		_consumer.Received += ConsumerReceivedAsync;
		_consumer.ConsumerCancelled += ConsumerCancelledAsync;

		_consumer.Model.EnsureQueue(_queueName, _durable, _autoDelete);
		_consumerTag = _consumer.Model.BasicConsume(_queueName, _autoAck, _consumer);

		Log.ConsumingConsumer(_logger, _queueName, _consumerTag);
	}

	private Task ConsumerReceivedAsync(object sender, BasicDeliverEventArgs @event)
		=> _handler?.Invoke(@event) ?? Task.CompletedTask;

	private Task ConsumerCancelledAsync(object sender, ConsumerEventArgs @event)
	{
		if (_consumer.ShutdownReason is null)
		{
			Log.LostConsumer(_logger, _consumerTag, _queueName);

			lock (_consumer.Model)
			{
				_consumer.Model.EnsureQueue(_queueName, _durable, _autoDelete);
				var consumerTag = _consumer.Model.BasicConsume(_queueName, _autoAck, _consumer);
				Log.RestoredConsumer(_logger, consumerTag, _queueName, _consumerTag);
				_consumerTag = consumerTag;
			}
		}

		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_consumer.Received -= ConsumerReceivedAsync;
		_consumer.ConsumerCancelled -= ConsumerCancelledAsync;

		_handler = null;
	}
}
