using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	internal static class ModelExtensions
	{
		/// <summary>
		/// Convenience method to consume a queue by declaring the exchange and queue and binding them together and starting to consume it with an <see cref="AsyncEventingBasicConsumer"/>.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="queueName">The name of the queue.</param>
		/// <param name="autoAck">The consumer should automatically acknowledge the message.</param>
		/// <param name="durable">The queue should survive a broker restart.</param>
		/// <param name="autoDelete">The queue should be deleted when the last consumer goes away.</param>
		/// <returns>An <see cref="AsyncEventingBasicConsumer"/> consuming the queue.</returns>
		public static AsyncEventingBasicConsumer ConsumeQueue(this IModel model, string queueName, bool autoAck = true, bool durable = true,
			bool autoDelete = true)
		{
			model.EnsureQueue(queueName, durable, autoDelete);

			var consumer = new AsyncEventingBasicConsumer(model);
			model.BasicConsume(queueName, autoAck, consumer);

			return consumer;
		}

		/// <summary>
		/// Convenience method to declare the exchange and queue.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="queueName">The name of the queue.</param>
		/// <param name="durable">The queue should survive a broker restart.</param>
		/// <param name="autoDelete">The queue should be deleted when the last consumer goes away.</param>
		private static void EnsureQueue(this IModel model, string queueName, bool durable = true, bool autoDelete = true)
		{
			model.ExchangeDeclare(Constants.ExchangeName, ExchangeType.Direct);
			model.QueueDeclare(queueName, autoDelete: autoDelete, durable: durable, exclusive: false);
			model.QueueBind(queueName, Constants.ExchangeName, queueName);
		}

		/// <summary>
		/// Convenience method to publish a payload as JSON to a queue. Ensures the existence of the queue when <paramref name="persistent"/> is set to true.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="queueName">The name of the queue.</param>
		/// <param name="payload">The payload to serialize as JSON and publish to the queue.</param>
		/// <param name="persistent">The publication should survive a broker restart (when the queue supports it).</param>
		/// <param name="durable">The queue should survive a broker restart.</param>
		/// <param name="autoDelete">The queue should be deleted when the last consumer goes away.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public static Task PublishJsonAsync(this IModel model, string queueName, object payload, bool persistent = true, bool durable = true,
			bool autoDelete = true)
		{
			var properties = model.CreateBasicProperties();
			properties.Persistent = persistent;
			properties.ContentType = "application/json";

			lock (model)
			{
				if (persistent)
				{
					model.EnsureQueue(queueName, durable, autoDelete);
				}

				model.BasicPublish(Constants.ExchangeName, queueName, properties, JsonSerializer.SerializeToUtf8Bytes(payload));
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Convenience method to remove consumers from their queue.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="consumerTags">The consumer tags the consumer is registered as.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public static Task CancelConsumerTagsAsync(this IModel model, IEnumerable<string> consumerTags)
		{
			foreach (var consumerTag in consumerTags)
			{
				model.BasicCancel(consumerTag);
			}

			return Task.CompletedTask;
		}

		/// <summary>
		/// Convenience method to remove consumers from their queue.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="consumerTags">The consumer tags the consumer is registered as.</param>
		public static void CancelConsumerTags(this IModel model, IEnumerable<string> consumerTags)
		{
			foreach (var consumerTag in consumerTags)
			{
				model.BasicCancel(consumerTag);
			}
		}

		/// <summary>
		/// Convenience method to acknowledge a message.
		/// </summary>
		/// <param name="model">The <see cref="IModel"/> used to communicate with Rabbit MQ.</param>
		/// <param name="deliveryTag">The delivery tag to acknowledge.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public static Task AcknowledgeAsync(this IModel model, ulong deliveryTag)
		{
			lock (model) model.BasicAck(deliveryTag, false);
			return Task.CompletedTask;
		}
	}
}
