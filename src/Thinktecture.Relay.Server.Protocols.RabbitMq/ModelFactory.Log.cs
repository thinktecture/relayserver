using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq;

public partial class ModelFactory<TAcknowledge>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ModelFactoryConnectionRecovered, LogLevel.Information,
			"Connection successful recovered")]
		public static partial void ConnectionRecovered(ILogger logger);

		[LoggerMessage(LoggingEventIds.ModelFactoryConnectionClosed, LogLevel.Debug,
			"Connection closed ({ShutdownReason})")]
		public static partial void ConnectionClosed(ILogger logger, string shutdownReason);

		[LoggerMessage(LoggingEventIds.ModelFactoryModelCreated, LogLevel.Trace,
			"Model created for {ModelContext} with channel {ModelChannel}")]
		public static partial void ModelCreated(ILogger logger, string modelContext, int modelChannel);

		[LoggerMessage(LoggingEventIds.ModelFactoryModelCallbackError, LogLevel.Error,
			"An error occured in a model callback for {ModelContext} with channel {ModelChannel}")]
		public static partial void ModelCallbackError(ILogger logger, Exception ex,
			string modelContext, int modelChannel);

		[LoggerMessage(LoggingEventIds.ModelFactoryModelClosed, LogLevel.Trace,
			"Model for {ModelContext} with channel {ModelChannel} closed ({ShutdownReason})")]
		public static partial void ModelClosed(ILogger logger, string modelContext, int modelChannel,
			string shutdownReason);
	}
}
