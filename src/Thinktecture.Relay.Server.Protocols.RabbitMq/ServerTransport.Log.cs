using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

public partial class ServerTransport<TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ServerTransportDispatchingAcknowledge, LogLevel.Trace,
			"Dispatching acknowledge {@AcknowledgeRequest}")]
		public static partial void DispatchingAcknowledge(ILogger logger, TAcknowledge acknowledgeRequest);

		[LoggerMessage(LoggingEventIds.ServerTransportDispatchedResponse, LogLevel.Trace,
			"Dispatched response for request {RelayRequestId} to origin {OriginId}")]
		public static partial void DispatchedResponse(ILogger logger, Guid relayRequestId, Guid originId);

		[LoggerMessage(LoggingEventIds.ServerTransportDispatchedAcknowledge, LogLevel.Trace,
			"Dispatched acknowledgement for request {RelayRequestId} to origin {OriginId}")]
		public static partial void DispatchedAcknowledge(ILogger logger, Guid relayRequestId, Guid originId);

		[LoggerMessage(LoggingEventIds.ServerTransportResponseConsumed, LogLevel.Trace,
			"Received response for request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
		public static partial void ResponseConsumed(ILogger logger, Guid relayRequestId, string queueName,
			string consumerTag);

		[LoggerMessage(LoggingEventIds.ServerTransportAcknowledgeConsumed, LogLevel.Trace,
			"Received acknowledge for request {RelayRequestId} from queue {QueueName} by consumer {ConsumerTag}")]
		public static partial void AcknowledgeConsumed(ILogger logger, Guid relayRequestId, string queueName,
			string consumerTag);
	}
}
