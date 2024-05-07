using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

internal partial class InMemoryBodyStore
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.InMemoryBodyStoreUsingStorage, LogLevel.Debug,
			"Using {StorageType} as body store")]
		public static partial void UsingStorage(ILogger logger, string storageType);
	}
}
