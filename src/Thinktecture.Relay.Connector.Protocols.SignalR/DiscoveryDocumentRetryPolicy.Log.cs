using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

public partial class DiscoveryDocumentRetryPolicy
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.DiscoveryDocumentRetryPolicyLogRetry, LogLevel.Information,
			"Connecting attempt {ConnectionAttempt} failed and will be tried again in {ReconnectDelay} seconds")]
		public static partial void LogRetry(ILogger logger, long connectionAttempt, int reconnectDelay);

		[LoggerMessage(LoggerEventIds.DiscoveryDocumentKeepingDefaults, LogLevel.Warning,
			"Keeping (default) reconnect delays because minimum ({ReconnectMinimumDelay}) cannot be greater than maximum ({ReconnectMaximumDelay})")]
		public static partial void KeepingDefaults(ILogger logger, TimeSpan? reconnectMinimumDelay,
			TimeSpan? reconnectMaximumDelay);

		[LoggerMessage(LoggerEventIds.DiscoveryDocumentUsingDelays, LogLevel.Debug,
			"Using a minimum of {ReconnectMinimumDelay} and a maximum of {ReconnectMaximumDelay} for reconnecting")]
		public static partial void UsingDelays(ILogger logger, TimeSpan? reconnectMinimumDelay,
			TimeSpan? reconnectMaximumDelay);
	}
}
