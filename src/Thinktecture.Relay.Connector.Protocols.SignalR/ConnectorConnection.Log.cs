using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

public partial class ConnectorConnection<TRequest, TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ConnectorConnectionDisconnecting, LogLevel.Trace,
			"Disconnecting connection {TransportConnectionId}")]
		public static partial void Disconnecting(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionDisconnected, LogLevel.Trace,
			"Disconnected on connection {TransportConnectionId}")]
		public static partial void Disconnected(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionGracefullyClosed, LogLevel.Debug,
			"Connection {TransportConnectionId} gracefully closed")]
		public static partial void ConnectionClosedGracefully(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionGracefullyClosed, LogLevel.Warning,
			"Connection {TransportConnectionId} closed")]
		public static partial void ConnectionClosed(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReconnectingAfterLoss, LogLevel.Information,
			"Trying to reconnect after connection {TransportConnectionId} was lost")]
		public static partial void ReconnectingAfterLoss(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReconnectingAfterError, LogLevel.Warning,
			"Trying to reconnect after connection {TransportConnectionId} was lost due to an error")]
		public static partial void ReconnectingAfterError(ILogger logger, Exception exception,
			string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReconnectedWithoutId, LogLevel.Warning,
			"Reconnected without a connection id")]
		public static partial void ReconnectedWithoutId(ILogger logger);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReconnected, LogLevel.Debug,
			"Reconnected on connection {TransportConnectionId}")]
		public static partial void Reconnected(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReconnectedWithNewId, LogLevel.Information,
			"Dropped connection {TransportConnectionId} in favor of new connection {NewTransportConnectionId}")]
		public static partial void ReconnectedWithNewId(ILogger logger, string transportConnectionId,
			string newTransportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionHandlingRequestDetailed, LogLevel.Trace,
			"Handling request {RelayRequestId} on connection {TransportConnectionId} {@Request}")]
		public static partial void HandlingRequestDetailed(ILogger logger, Guid relayRequestId,
			string transportConnectionId, TRequest request);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionHandlingRequestSimple, LogLevel.Debug,
			"Handling request {RelayRequestId} on connection {TransportConnectionId} from origin {OriginId}")]
		public static partial void HandlingRequestSimple(ILogger logger, Guid relayRequestId,
			string transportConnectionId, Guid originId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionReceivedTenantConfig, LogLevel.Trace,
			"Received tenant config {@Config} on connection {TransportConnectionId}")]
		public static partial void ReceivedTenantConfig(ILogger logger, ITenantConfig config,
			string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionLogConnected, LogLevel.Information,
			"Connected on connection {TransportConnectionId}")]
		public static partial void LogConnected(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorConnectionConnectError, LogLevel.Error,
			"An error occured while trying to connect")]
		public static partial void ConnectError(ILogger logger, Exception exception);
	}
}
