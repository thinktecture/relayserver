using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR;

public partial class ConnectorTransport<TRequest, TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ConnectorTransportTransportingRequest, LogLevel.Trace,
			"Transporting request {@Request} for request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void TransportingRequest(ILogger logger, IClientRequest request, Guid relayRequestId,
			string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorTransportErrorTransportingRequest, LogLevel.Error,
			"An error occured while transporting request {RelayRequestId} on connection {TransportConnectionId}")]
		public static partial void ErrorTransportingRequest(ILogger logger, Exception ex, Guid relayRequestId,
			string transportConnectionId);
	}
}
