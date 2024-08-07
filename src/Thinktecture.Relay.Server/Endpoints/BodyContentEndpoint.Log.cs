using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Endpoints;

internal partial class BodyContentEndpoint
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.BodyContentEndpointDeliverBody, LogLevel.Debug,
			"Delivering request body content for request {RelayRequestId}, should delete: {DeleteBody}")]
		public static partial void DeliverBody(ILogger logger, Guid relayRequestId, bool deleteBody);

		[LoggerMessage(LoggingEventIds.BodyContentEndpointStoreBody, LogLevel.Debug,
			"Storing response body content for request {RelayRequestId}")]
		public static partial void StoreBody(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.BodyContentEndpointResponseAborted, LogLevel.Warning,
			"Connector for {TenantName} aborted the response body upload for request {RelayRequestId}")]
		public static partial void ResponseAborted(ILogger logger, string tenantName, Guid relayRequestId);
	}
}
