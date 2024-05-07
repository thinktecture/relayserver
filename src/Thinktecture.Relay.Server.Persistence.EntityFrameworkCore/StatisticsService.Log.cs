using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Persistence.EntityFrameworkCore;

public partial class StatisticsService
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.StatisticsServiceAddingNewOrigin, LogLevel.Debug,
			"Adding new origin {OriginId} to statistics tracking")]
		public static partial void AddingNewOrigin(ILogger logger, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorCreatingOrigin, LogLevel.Error,
			"An error occured while creating origin {OriginId} for statistics tracking")]
		public static partial void ErrorCreatingOrigin(ILogger logger, Exception ex, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceUpdateLastSeen, LogLevel.Debug,
			"Updating last seen time of origin {OriginId} in statistics tracking")]
		public static partial void UpdateLastSeen(ILogger logger, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorUpdatingOrigin, LogLevel.Error,
			"An error occured while updating origin {OriginId} for statistics tracking")]
		public static partial void ErrorUpdatingOrigin(ILogger logger, Exception ex, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceSettingShutdownTime, LogLevel.Debug,
			"Setting shutdown time of origin {OriginId} in statistics tracking")]
		public static partial void SettingShutdownTime(ILogger logger, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorSettingShutdownTime, LogLevel.Error,
			"An error occured while setting shutdown time of origin {OriginId} for statistics tracking")]
		public static partial void ErrorSettingShutdownTime(ILogger logger, Exception ex, Guid originId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceCleanup, LogLevel.Debug,
			"Cleaning up statistics storage by deleting all origins that have not been seen since {OriginLastSeen}")]
		public static partial void Cleanup(ILogger logger, DateTimeOffset originLastSeen);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorDeletingOrigins, LogLevel.Error,
			"An error occured while deleting old origins")]
		public static partial void ErrorDeletingOrigins(ILogger logger, Exception ex);

		[LoggerMessage(LoggingEventIds.StatisticsServiceAddingNewConnection, LogLevel.Debug,
			"Adding new connection {TransportConnectionId} for statistics tracking")]
		public static partial void AddingNewConnection(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorCreatingConnection, LogLevel.Error,
			"An error occured while creating connection {TransportConnectionId} for statistics tracking")]
		public static partial void ErrorCreatingConnection(ILogger logger, Exception ex, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceUpdateConnectionLastSeenTime, LogLevel.Debug,
			"Updating last seen time of connection {TransportConnectionId} to {LastSeenTime} within batch {UpdateBatchId} in statistics tracking")]
		public static partial void UpdateConnectionLastSeenTime(ILogger logger, string transportConnectionId,
			DateTimeOffset lastSeenTime, Guid updateBatchId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceUpdateConnectionsLastSeenTime, LogLevel.Debug,
			"Starting batch {UpdateBatchId} to update the last seen time of {UpdateAmount} connections")]
		public static partial void UpdateConnectionsLastSeenTime(ILogger logger, Guid updateBatchId, int updateAmount);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorUpdatingConnections, LogLevel.Error,
			"An error occured while updating last seen time of multiple connections in batch {UpdateBatchId} in statistics tracking")]
		public static partial void ErrorUpdatingConnections(ILogger logger, Exception ex, Guid updateBatchId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceSettingDisconnectTime, LogLevel.Debug,
			"Setting disconnect time of connection {TransportConnectionId} in statistics tracking")]
		public static partial void SettingDisconnectTime(ILogger logger, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorSettingDisconnectTime, LogLevel.Error,
			"An error occured while setting disconnect time of connection {TransportConnectionId} for statistics tracking")]
		public static partial void ErrorSettingDisconnectTime(ILogger logger, Exception ex, string transportConnectionId);

		[LoggerMessage(LoggingEventIds.StatisticsServiceConnectionCleanup, LogLevel.Debug,
			"Cleaning up statistics storage by deleting all connections that have no activity or are disconnected since {ConnectionLastActivity}")]
		public static partial void ConnectionCleanup(ILogger logger, DateTimeOffset connectionLastActivity);

		[LoggerMessage(LoggingEventIds.StatisticsServiceErrorCleaningUpConnections, LogLevel.Error,
			"An error occured while cleaning up old connections")]
		public static partial void ErrorCleaningUpConnections(ILogger logger, Exception ex);
	}
}
