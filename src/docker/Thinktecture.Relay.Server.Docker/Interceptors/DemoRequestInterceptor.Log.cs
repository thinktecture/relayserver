using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public partial class DemoRequestInterceptor
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.DemoRequestInterceptorRemovingOutput, LogLevel.Information,
			"Request stream interceptor enabled for request {RequestId}, input was {OriginalRequestSize}, output will be NULL")]
		public static partial void RemovingOutput(ILogger logger, Guid requestId, long? originalRequestSize);

		[LoggerMessage(LoggingEventIds.DemoRequestInterceptorEnabled, LogLevel.Information,
			"Request stream interceptor enabled for request {RequestId}, input was {OriginalRequestSize}, output will be {RequestSize} bytes")]
		public static partial void Enabled(ILogger logger, Guid requestId, long? originalRequestSize, int requestSize);
	}
}
