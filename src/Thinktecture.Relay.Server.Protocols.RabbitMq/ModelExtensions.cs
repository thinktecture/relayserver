using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal static class ModelExtensions
{
	/// <summary>
	/// Convenience method to declare the exchange and queue.
	/// </summary>
	/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
	/// <param name="queueName">The name of the queue.</param>
	/// <param name="durable">The queue should survive a broker restart.</param>
	/// <param name="autoDelete">The queue should be deleted when the last consumer goes away.</param>
	public static void EnsureQueue(this IModel model, string queueName, bool durable = true, bool autoDelete = false)
	{
		model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct);
		model.QueueDeclare(queueName, durable: durable, exclusive: false, autoDelete: autoDelete);
		model.QueueBind(queueName, Constants.ExchangeName, queueName);
	}

	/// <summary>
	/// Convenience method to publish a payload as JSON to a queue.
	/// </summary>
	/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
	/// <param name="queueName">The name of the queue.</param>
	/// <param name="payload">The payload to serialize as JSON and publish to the queue.</param>
	/// <param name="persistent">The publication should survive a broker restart (when the queue supports it).</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public static Task PublishJsonAsync(this IModel model, string queueName, object payload, bool persistent = false)
	{
		var properties = model.CreateBasicProperties();
		properties.Persistent = persistent;
		properties.ContentType = "application/json";

		var body = JsonSerializer.SerializeToUtf8Bytes(payload);

		lock (model)
		{
			model.BasicPublish(Constants.ExchangeName, queueName, properties, body);
		}

		return Task.CompletedTask;
	}

	/// <summary>
	/// Convenience method to acknowledge a message.
	/// </summary>
	/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
	/// <param name="deliveryTag">The delivery tag to acknowledge.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public static Task AcknowledgeAsync(this IModel model, ulong deliveryTag)
	{
		lock (model)
		{
			model.BasicAck(deliveryTag, false);
		}

		return Task.CompletedTask;
	}
}
