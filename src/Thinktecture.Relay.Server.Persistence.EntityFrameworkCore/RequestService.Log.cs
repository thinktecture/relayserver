using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

public partial class RequestService
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RequestServiceStoringRequest, LogLevel.Trace, "Storing request {@Request}")]
		public static partial void StoringRequest(ILogger logger, Request request);

		[LoggerMessage(LoggingEventIds.RequestServiceErrorStoringRequest, LogLevel.Error,
			"An error occured while storing request {RelayRequestId}")]
		public static partial void ErrorStoringRequest(ILogger logger, Exception exception, Guid relayRequestId);
	}
}
