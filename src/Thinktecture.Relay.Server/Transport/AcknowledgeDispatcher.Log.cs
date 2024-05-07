using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

public partial class AcknowledgeDispatcher<TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AcknowledgeDispatcherLocalAcknowledge, LogLevel.Trace,
			"Locally dispatching acknowledge for request {RelayRequestId}")]
		public static partial void LocalAcknowledge(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.AcknowledgeDispatcherRedirectAcknowledge, LogLevel.Trace,
			"Remotely dispatching acknowledge for request {RelayRequestId} to origin {OriginId}")]
		public static partial void RedirectAcknowledge(ILogger logger, Guid relayRequestId, Guid originId);
	}
}
