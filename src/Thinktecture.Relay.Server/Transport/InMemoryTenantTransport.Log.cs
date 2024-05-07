using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

internal partial class InMemoryTenantTransport<T>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.InMemoryTenantTransportErrorDeliveringRequest, LogLevel.Error,
			"Could not deliver request {RelayRequestId} to a connection")]
		public static partial void ErrorDeliveringRequest(ILogger logger, Guid relayRequestId);
	}
}
