using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Targets;

public partial class RelayTargetRegistry<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RelayTargetRegistryRegisteredTarget, LogLevel.Debug,
			"Registered relay target {Target} as type {TargetType}")]
		public static partial void RegisteredTarget(ILogger logger, string target, string? targetType);

		[LoggerMessage(LoggingEventIds.RelayTargetRegistryUnregisteredTarget, LogLevel.Debug,
			"Unregistered relay target {Target}")]
		public static partial void UnregisteredTarget(ILogger logger, string target);

		[LoggerMessage(LoggingEventIds.RelayTargetRegistryCouldNotUnregisterTarget, LogLevel.Warning,
			"Could not unregister relay target {Target}")]
		public static partial void CouldNotUnregisterTarget(ILogger logger, string target);
	}
}
