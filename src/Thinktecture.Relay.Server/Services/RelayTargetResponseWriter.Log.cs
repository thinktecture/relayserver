using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Services;

public partial class RelayTargetResponseWriter<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RelayTargetResponseWriterFailedRequest, LogLevel.Warning,
			"The request {RelayRequestId} failed internally with {HttpStatusCode}")]
		public static partial void FailedRequest(ILogger logger, Guid relayRequestId, HttpStatusCode httpStatusCode);
	}
}
