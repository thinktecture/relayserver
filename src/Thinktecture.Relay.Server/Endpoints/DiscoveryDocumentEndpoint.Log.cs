using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Endpoints;

internal partial class DiscoveryDocumentEndpoint
{
    private static partial class Log
    {
        [LoggerMessage(LoggingEventIds.DiscoveryDocumentEndpointReturnDiscoveryDocument, LogLevel.Debug,
            "Returning discovery document")]
        public static partial void ReturnDiscoveryDocument(ILogger logger);
    }
}
