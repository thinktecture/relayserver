using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

public partial class AcknowledgeTransport<T>
{
	private partial class Log
	{
		[LoggerMessage(LoggingEventIds.AcknowledgeTransportTransportingAck, LogLevel.Trace,
			"Transporting acknowledge request {@AcknowledgeRequest} for request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void TransportingAck(ILogger logger, IAcknowledgeRequest acknowledgeRequest,
			Guid relayRequestId, string? transportConnectionId);

		[LoggerMessage(LoggingEventIds.AcknowledgeTransportErrorTransportingAck, LogLevel.Error,
			"An error occured while transporting acknowledge for request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void ErrorTransportingAck(ILogger logger, Exception? ex, Guid relayRequestId,
			string? transportConnectionId);
	}
}
