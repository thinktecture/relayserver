using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Endpoints;

internal partial class AcknowledgeEndpoint
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AcknowledgeEndpointAcknowledgementReceived, LogLevel.Debug,
			"Received acknowledgement for request {RelayRequestId} on origin {OriginId}")]
		public static partial void AcknowledgementReceived(ILogger logger, Guid relayRequestId, Guid originId);
	}
}
