using System;
using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Options;

internal partial class RelayConnectorPostConfigureOptions<TRequest, TResponse>
{
	public static partial class Log
	{
		[LoggerMessage(LoggerEventIds.RelayConnectorPostConfigureOptionsGotDiscoveryDocument, LogLevel.Trace,
			"Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})")]
		public static partial void GotDiscoveryDocument(ILogger logger, Uri discoveryDocumentUrl,
			DiscoveryDocument discoveryDocument);

		[LoggerMessage(LoggerEventIds.AccessTokenManagementConfigureOptionsErrorRetrievingDiscoveryDocument,
			LogLevel.Error,
			"An error occured while retrieving the discovery document from {DiscoveryDocumentUrl}")]
		public static partial void ErrorRetrievingDiscoveryDocument(ILogger logger, Exception exception,
			string discoveryDocumentUrl);

		[LoggerMessage(LoggerEventIds.AccessTokenManagementConfigureOptionsErrorTargetTypeNotFound, LogLevel.Error,
			"Could not find target type {TargetType} for target {Target}")]
		public static partial void ErrorTargetTypeNotFound(ILogger logger, string targetType, string target);

		[LoggerMessage(LoggerEventIds.AccessTokenManagementConfigureOptionsCouldNotParseTimeout, LogLevel.Warning,
			"Could not parse timeout \"{TargetTimeout}\" for target {Target}")]
		public static partial void CouldNotParseTimeout(ILogger logger, string targetTimeout, string target);
	}
}
