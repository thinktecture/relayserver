using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

public partial class ResponseTransport<T>
{
	private partial class Log
	{
		[LoggerMessage(LoggingEventIds.ResponseTransportTransportingResponse, LogLevel.Trace,
			"Transporting response {@Response} for request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void TransportingResponse(ILogger logger, ITargetResponse response, Guid relayRequestId,
			string? transportConnectionId);

		[LoggerMessage(LoggingEventIds.ResponseTransportErrorTransportingResponse, LogLevel.Error,
			"An error occured while transporting response for request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void ErrorTransportingResponse(ILogger logger, Exception exception,
			Guid relayRequestId, string? transportConnectionId);
	}
}
