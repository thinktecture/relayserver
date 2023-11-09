using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Targets;

public partial class ClientRequestHandler<TRequest, TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.ClientRequestHandlerAcknowledgeRequest, LogLevel.Debug,
			"Acknowledging request {RelayRequestId} on origin {OriginId}")]
		public static partial void AcknowledgeRequest(ILogger logger, Guid relayRequestId, Guid? originId);

		[LoggerMessage(LoggerEventIds.ClientRequestHandlerErrorHandlingRequest, LogLevel.Error,
			"An error occured during handling of request {RelayRequestId}")]
		public static partial void ErrorHandlingRequest(ILogger logger, Exception exception, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestHandlerDeliverResponse, LogLevel.Debug,
			"Delivering response for request {RelayRequestId}")]
		public static partial void DeliverResponse(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggerEventIds.ClientRequestHandlerDiscardResponse, LogLevel.Debug,
			"Discarding response for request {RelayRequestId}")]
		public static partial void DiscardResponse(ILogger logger, Guid relayRequestId);
	}
}
