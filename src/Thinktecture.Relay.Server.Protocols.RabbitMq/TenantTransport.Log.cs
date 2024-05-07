using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

public partial class TenantTransport<TRequest, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.TenantTransportPublishedRequest, LogLevel.Trace,
			"Published request {RelayRequestId} to tenant {TenantName}")]
		public static partial void PublishedRequest(ILogger logger, Guid relayRequestId, string tenantName);

		[LoggerMessage(LoggingEventIds.TenantTransportErrorDispatchingRequest, LogLevel.Error,
			"An error occured while dispatching request {RelayRequestId} to tenant {TenantName} queue")]
		public static partial void ErrorDispatchingRequest(ILogger logger, Exception ex, Guid relayRequestId,
			string tenantName);
	}
}
