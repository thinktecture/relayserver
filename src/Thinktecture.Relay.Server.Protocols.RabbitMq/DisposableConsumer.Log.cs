using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

internal partial class DisposableConsumer
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ModelExtensionsConsumingConsumer, LogLevel.Trace,
			"Consuming {QueueName} with consumer {ConsumerTag}")]
		public static partial void ConsumingConsumer(ILogger logger, string queueName, string? consumerTag);

		[LoggerMessage(LoggingEventIds.ModelExtensionsLostConsumer, LogLevel.Warning,
			"Lost consumer {ConsumerTag} on queue {QueueName}")]
		public static partial void LostConsumer(ILogger logger, string consumerTag, string queueName);

		[LoggerMessage(LoggingEventIds.ModelExtensionsRestoredConsumer, LogLevel.Information,
			"Restored consumer {ConsumerTag} on queue {QueueName} (was {OldConsumerTag})")]
		public static partial void RestoredConsumer(ILogger logger, string consumerTag, string queueName,
			string oldConsumerTag);
	}
}
