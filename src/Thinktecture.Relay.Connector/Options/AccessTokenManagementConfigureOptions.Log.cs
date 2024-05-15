using System;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Thinktecture.Relay.Connector.Options;

internal partial class AccessTokenManagementConfigureOptions
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AccessTokenManagementConfigureOptionsGotDiscoveryDocument, LogLevel.Trace,
			"Got discovery document from {DiscoveryDocumentUrl} ({@DiscoveryDocument})")]
		public static partial void GotDiscoveryDocument(ILogger logger, Uri discoveryDocumentUrl,
			OpenIdConnectConfiguration discoveryDocument);

		[LoggerMessage(LoggingEventIds.AccessTokenManagementConfigureOptionsErrorRetrievingDiscoveryDocument,
			LogLevel.Error,
			"An error occured while retrieving the discovery document from {DiscoveryDocumentUrl}")]
		public static partial void ErrorRetrievingDiscoveryDocument(ILogger logger,
			Exception exception, Uri discoveryDocumentUrl);
	}
}
