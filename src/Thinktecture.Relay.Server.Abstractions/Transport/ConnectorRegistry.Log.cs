using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

public partial class ConnectorRegistry<T>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ConnectorRegistryRegisteringConnection, LogLevel.Debug,
			"Registering connection {TransportConnectionId} for tenant {TenantName}")]
		public static partial void RegisteringConnection(ILogger logger, string transportConnectionId,
			string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorRegistryUnregisteringConnection, LogLevel.Debug,
			"Unregistering connection {TransportConnectionId} for tenant {TenantName}")]
		public static partial void UnregisteringConnection(ILogger logger, string transportConnectionId,
			string tenantName);

		[LoggerMessage(LoggingEventIds.ConnectorRegistryCouldNotUnregisterConnection, LogLevel.Warning,
			"Could not unregister connection {TransportConnectionId}")]
		public static partial void CouldNotUnregisterConnection(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.ConnectorRegistryUnknownRequestConnection, LogLevel.Warning,
			"Unknown connection {TransportConnectionId} to transport request {RelayRequestId} to")]
		public static partial void UnknownRequestConnection(ILogger logger, string transportConnectionId,
			Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.ConnectorRegistryUnknownAcknowledgeConnection, LogLevel.Warning,
			"Unknown connection {TransportConnectionId} to transport acknowledge {AcknowledgeId} to")]
		public static partial void UnknownAcknowledgeConnection(ILogger logger, string transportConnectionId,
			string acknowledgeId);

		[LoggerMessage(LoggingEventIds.ConnectorRegistryDeliveringRequest, LogLevel.Trace,
			"Delivering request {RelayRequestId} to local connection {TransportConnectionId}")]
		public static partial void DeliveringRequest(ILogger logger, Guid relayRequestId, string? transportConnectionId);
	}
}
