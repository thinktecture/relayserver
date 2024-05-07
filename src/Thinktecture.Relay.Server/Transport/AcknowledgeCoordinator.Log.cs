using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

public partial class AcknowledgeCoordinator<TRequest, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AcknowledgeCoordinatorRegisterAcknowledgeState, LogLevel.Trace,
			"Registering acknowledge state of request {RelayRequestId} from connection {TransportConnectionId} for id {AcknowledgeId}")]
		public static partial void RegisterAcknowledgeState(ILogger logger, Guid relayRequestId,
			string transportConnectionId, string acknowledgeId);

		[LoggerMessage(LoggingEventIds.AcknowledgeCoordinatorReRegisterAcknowledgeState, LogLevel.Warning,
			"Re-registering an already existing request {RelayRequestId} from connection {TransportConnectionId} for id {AcknowledgeId} – this can happen when a request got queued again and will lead to a re-execution of the request but only the first result be acknowledged and the others will be discarded")]
		public static partial void ReRegisterAcknowledgeState(ILogger logger, Guid relayRequestId,
			string transportConnectionId, string acknowledgeId);

		[LoggerMessage(LoggingEventIds.AcknowledgeCoordinatorUnknownRequest, LogLevel.Warning,
			"Unknown request {RelayRequestId} to acknowledge received")]
		public static partial void UnknownRequest(ILogger logger, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.AcknowledgeCoordinatorPrunedRequest, LogLevel.Debug,
			"Request {RelayRequestId} was already pruned and will not be acknowledged – this happens after an auto-recovery of the message queue transport")]
		public static partial void PrunedRequest(ILogger logger, Guid relayRequestId);
	}
}
