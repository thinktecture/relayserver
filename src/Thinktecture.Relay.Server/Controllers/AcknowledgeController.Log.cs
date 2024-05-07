using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Controllers;

public partial class AcknowledgeController
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AcknowledgeControllerAcknowledgementReceived, LogLevel.Debug,
			"Received acknowledgement for request {RelayRequestId} on origin {OriginId}")]
		public static partial void AcknowledgementReceived(ILogger logger, Guid relayRequestId, Guid originId);
	}
}
