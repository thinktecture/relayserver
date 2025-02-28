using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Options;

internal partial class RelayConnectorPostConfigureOptions<TRequest, TResponse>
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.RelayConnectorPostConfigureOptionsGotDiscoveryDocument, LogLevel.Trace,
			"Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})")]
		public static partial void GotDiscoveryDocument(ILogger logger, Uri discoveryDocumentUrl,
			DiscoveryDocument discoveryDocument);

		[LoggerMessage(LoggingEventIds.RelayConnectorPostConfigureOptionsErrorRetrievingDiscoveryDocument,
			LogLevel.Error,
			"An error occured while retrieving the discovery document from {DiscoveryDocumentUrl}")]
		public static partial void ErrorRetrievingDiscoveryDocument(ILogger logger, Exception exception,
			string discoveryDocumentUrl);

		[LoggerMessage(LoggingEventIds.RelayConnectorPostConfigureOptionsErrorTargetTypeNotFound, LogLevel.Error,
			"Could not find target type {TargetType} for target {Target}")]
		public static partial void ErrorTargetTypeNotFound(ILogger logger, string targetType, string target);

		[LoggerMessage(LoggingEventIds.RelayConnectorPostConfigureOptionsCouldNotParseTimeout, LogLevel.Warning,
			"Could not parse timeout \"{TargetTimeout}\" for target {Target}")]
		public static partial void CouldNotParseTimeout(ILogger logger, string targetTimeout, string target);
	}
}
