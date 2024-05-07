using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

public partial class RequestCoordinator<T>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RequestCoordinatorRedirect, LogLevel.Debug,
			"Redirecting request {RelayRequestId} to transport for tenant {TenantName}")]
		public static partial void Redirect(ILogger logger, Guid relayRequestId, string tenantName);
	}
}
