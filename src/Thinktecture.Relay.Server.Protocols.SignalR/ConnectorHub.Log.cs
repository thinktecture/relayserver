using System;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence.Models;

namespace Thinktecture.Relay.Server.Protocols.SignalR;

public partial class ConnectorHub<TRequest, TResponse, TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ConnectorHubErrorNoTenantName, LogLevel.Error,
			"Rejecting incoming connection {TransportConnectionId} because of missing tenant name")]
		public static partial void ErrorNoTenantName(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorHubIncomingConnectionCreatedTenant, LogLevel.Information,
			"Incoming connection {TransportConnectionId} created tenant {TenantName}")]
		public static partial void IncomingConnectionCreatedTenant(
			ILogger logger, string transportConnectionId,
			string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorHubRejectingUnknownTenant, LogLevel.Error,
			"Rejecting incoming connection {TransportConnectionId} because of unknown tenant {TenantName}")]
		public static partial void
			RejectingUnknownTenant(ILogger logger, string transportConnectionId, string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorHubIncomingConnectionUpdatedTenant, LogLevel.Information,
			"Incoming connection {TransportConnectionId} updated tenant {TenantName}")]
		public static partial void IncomingConnectionUpdatedTenant(ILogger logger, string transportConnectionId,
			string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorHubIncomingConnection, LogLevel.Debug,
			"Incoming connection {TransportConnectionId} for tenant {@Tenant}")]
		public static partial void IncomingConnection(ILogger logger, string transportConnectionId, Tenant tenant);

		[LoggerMessage(LoggingEventIds.ConnectorHubDisconnectedError, LogLevel.Warning,
			"Connection {TransportConnectionId} disconnected for tenant {TenantName}")]
		public static partial void DisconnectedError(ILogger logger, Exception ex, string transportConnectionId,
			string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorHubDisconnected, LogLevel.Debug,
			"Connection {TransportConnectionId} disconnected for tenant {TenantName}")]
		public static partial void Disconnected(ILogger logger, string transportConnectionId, string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorHubReceivedResponse, LogLevel.Debug,
			"Connection {TransportConnectionId} received response for request {RelayRequestId}")]
		public static partial void ReceivedResponse(ILogger logger, string transportConnectionId, Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ConnectorHubReceivedAcknowledge, LogLevel.Debug,
			"Connection {TransportConnectionId} received acknowledgement for request {RelayRequestId}")]
		public static partial void ReceivedAcknowledge(ILogger logger, string transportConnectionId, Guid relayRequestId);
	}
}
