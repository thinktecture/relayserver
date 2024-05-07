using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

public partial class TenantHandler<TRequest, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.TenantHandlerAcknowledge, LogLevel.Trace, "Acknowledging {AcknowledgeId}")]
		public static partial void Acknowledge(ILogger logger, string acknowledgeId);

		[LoggerMessage(LoggingEventIds.TenantHandlerCouldNotParseAcknowledge, LogLevel.Warning,
			"Could not parse acknowledge id {AcknowledgeId}")]
		public static partial void CouldNotParseAcknowledge(ILogger logger, string acknowledgeId);

		[LoggerMessage(LoggingEventIds.TenantHandlerReceivedRequest, LogLevel.Trace,
			"Received request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
		public static partial void ReceivedRequest(ILogger logger, Guid relayRequestId, string queueName,
			string consumerTag);
	}
}
