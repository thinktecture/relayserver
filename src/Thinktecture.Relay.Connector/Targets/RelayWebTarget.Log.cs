using System;
using System.Net;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Targets;

public partial class RelayWebTarget<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.RelayWebTargetRequestingTarget, LogLevel.Trace,
			"Requesting target for request {RelayRequestId} at {BaseAddress} for {Url}")]
		public static partial void RequestingTarget(ILogger logger, Guid relayRequestId, Uri? baseAddress, string url);

		[LoggerMessage(LoggerEventIds.RelayWebTargetRequestedTarget, LogLevel.Debug,
			"Requested target for request {RelayRequestId} returned {HttpStatusCode}")]
		public static partial void RequestedTarget(ILogger logger, Guid relayRequestId, HttpStatusCode httpStatusCode);
	}
}
