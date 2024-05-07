using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

public partial class ResponseCoordinator<T>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ResponseCoordinatorRequestAlreadyRegistered, LogLevel.Error,
			"Request {RelayRequestId} is already registered")]
		public static partial void RequestAlreadyRegistered(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorWaitingForResponse, LogLevel.Debug,
			"Waiting for response for request {RelayRequestId}")]
		public static partial void WaitingForResponse(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorNoWaitingStateFound, LogLevel.Debug,
			"No waiting state for request {RelayRequestId} found")]
		public static partial void NoWaitingStateFound(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorCancelingWait, LogLevel.Trace,
			"Canceling response wait for request {RelayRequestId}")]
		public static partial void CancelingWait(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorBodyOpened, LogLevel.Debug,
			"Opened outsourced response body for request {RelayRequestId} with {BodySize} bytes")]
		public static partial void BodyOpened(ILogger logger, Guid relayRequestId, long? bodySize);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorInlinedReceived, LogLevel.Debug,
			"Response with inlined body for request {RelayRequestId} received")]
		public static partial void InlinedReceived(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorNoBodyReceived, LogLevel.Debug,
			"Response for request {RelayRequestId} without body received")]
		public static partial void NoBodyReceived(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorResponseReceived, LogLevel.Trace,
			"Response {@Response} for request {RelayRequestId} received")]
		public static partial void ResponseReceived(ILogger logger, ITargetResponse response, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseCoordinatorResponseDiscarded, LogLevel.Debug,
			"Response for request {RelayRequestId} discarded")]
		public static partial void ResponseDiscarded(ILogger logger, Guid relayRequestId);
	}
}
