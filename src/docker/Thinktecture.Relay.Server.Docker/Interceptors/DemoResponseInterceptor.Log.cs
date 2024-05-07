using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Docker.Interceptors;

public partial class DemoResponseInterceptor
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.DemoResponseInterceptorRemovingOutput, LogLevel.Information,
			"Response stream interceptor enabled for request {RequestId}, input was {OriginalResponseSize}, output will be NULL")]
		public static partial void RemovingOutput(ILogger logger, Guid requestId, long? originalResponseSize);

		[LoggerMessage(LoggingEventIds.DemoResponseInterceptorEnabled, LogLevel.Information,
			"Response stream interceptor enabled for request {RequestId}, input was {OriginalResponseSize}, output will be {ResponseSize} bytes")]
		public static partial void Enabled(ILogger logger, Guid requestId, long? originalResponseSize,
			long? responseSize);
	}
}
