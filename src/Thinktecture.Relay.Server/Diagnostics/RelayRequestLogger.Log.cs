using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Diagnostics;

public partial class RelayRequestLogger<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(20400, LogLevel.Trace, "Writing request log for successful request {RelayRequestId}")]
		public static partial void Success(ILogger logger, Guid relayRequestId);

		[LoggerMessage(20401, LogLevel.Trace, "Writing request log for aborted request {RelayRequestId}")]
		public static partial void Abort(ILogger logger, Guid relayRequestId);

		[LoggerMessage(20402, LogLevel.Trace, "Writing request log for failed request {RelayRequestId}")]
		public static partial void Fail(ILogger logger, Guid relayRequestId);

		[LoggerMessage(20403, LogLevel.Trace, "Writing request log for expired request {RelayRequestId}")]
		public static partial void Expired(ILogger logger, Guid relayRequestId);

		[LoggerMessage(20404, LogLevel.Trace, "Writing request log for error on request {RelayRequestId}")]
		public static partial void Error(ILogger logger, Guid relayRequestId);
	}
}
