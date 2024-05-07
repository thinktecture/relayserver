using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Server.Controllers;
public partial class DiscoveryDocumentController
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.DiscoveryDocumentControllerReturnDiscoveryDocument, LogLevel.Debug,
			"Returning discovery document")]
		public static partial void ReturnDiscoveryDocument(ILogger logger);
	}
}
