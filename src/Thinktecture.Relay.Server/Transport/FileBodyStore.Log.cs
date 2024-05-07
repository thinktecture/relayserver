using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Transport;

internal partial class FileBodyStore
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.FileBodyStoreUsingStorage, LogLevel.Debug,
			"Using {StorageType} with storage path {StoragePath} as body store")]
		public static partial void UsingStorage(ILogger logger, string storageType, string storagePath);

		[LoggerMessage(LoggingEventIds.FileBodyStoreOperation, LogLevel.Trace,
			"{FileOperation} {FileBodyType} body for request {RelayRequestId}")]
		public static partial void Operation(ILogger logger, string fileOperation, string fileBodyType,
			Guid relayRequestId);

		[LoggerMessage(LoggingEventIds.FileBodyStoreWriting, LogLevel.Debug,
			"Writing of {FileBodyType} body for request {RelayRequestId} completed with {BodySize} bytes")]
		public static partial void Writing(ILogger logger, string fileBodyType, Guid relayRequestId, long bodySize);

		[LoggerMessage(LoggingEventIds.FileBodyStoreError, LogLevel.Warning,
			"An error occured while {FileOperation} {FileBodyType} body for request {RelayRequestId}")]
		public static partial void Error(ILogger logger, Exception ex, string fileOperation, string fileBodyType,
			Guid relayRequestId);
	}
}

internal partial class FileBodyStoreValidateOptions
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.FileBodyStoreValidateOptionsErrorCheckingPermissions, LogLevel.Error,
			"An error occured while checking file creation, read and write permission on configured body store path {Path}")]
		public static partial void ErrorCheckingPermissions(ILogger logger, Exception ex, string path);
	}
}
