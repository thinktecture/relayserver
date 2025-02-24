using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Options;

internal partial class ClientCredentialsClientConfigureOptions
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.ClientCredentialsClientConfigureOptionsSetTokenEndpoint, LogLevel.Trace,
			"Using token endpoint {TokenEndpoint}")]
		public static partial void UseTokenEndpoint(ILogger logger, string tokenEndpoint);
	}
}
