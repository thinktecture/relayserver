using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

public partial class ResponseDispatcher<TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ResponseDispatcherLocalDispatch, LogLevel.Trace,
			"Locally dispatching response for request {RelayRequestId}")]
		public static partial void LocalDispatch(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ResponseDispatcherRedirectDispatch, LogLevel.Trace,
			"Remotely dispatching response for request {RelayRequestId} to origin {OriginId}")]
		public static partial void RedirectDispatch(ILogger logger, Guid relayRequestId, Guid originId);
	}
}
