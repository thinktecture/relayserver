using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Docker;

internal partial class InProcFunc
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.InProcFuncExecuting, LogLevel.Information,
			"Executing demo in proc target for request {RequestId}")]
		public static partial void Executing(ILogger logger, Guid requestId);

		[LoggerMessage(LoggingEventIds.InProcFuncBodySize, LogLevel.Information,
			"Request {RequestId} provided {BodySize} bytes in body")]
		public static partial void BodySize(ILogger logger, Guid requestId, long? bodySize);

		[LoggerMessage(LoggingEventIds.InProcFuncEcho, LogLevel.Information,
			"Demo in proc target is ECHOING received request body")]
		public static partial void Echo(ILogger logger);
	}
}

internal partial class ConnectorService
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ConnectorServiceStarting, LogLevel.Information, "Starting connector")]
		public static partial void Starting(ILogger logger);

		[LoggerMessage(LoggingEventIds.ConnectorServiceStopping, LogLevel.Information, "Gracefully stopping connector")]
		public static partial void Stopping(ILogger logger);
	}
}
