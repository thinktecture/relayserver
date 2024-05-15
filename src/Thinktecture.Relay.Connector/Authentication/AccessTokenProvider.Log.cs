using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Authentication;

internal partial class AccessTokenProvider
{
	private static partial class Log
	{
		[LoggerMessage(LoggingEventIds.AccessTokenProviderRequestingAccessToken, LogLevel.Debug, "Requesting access token")]
		public static partial void RequestingAccessToken(ILogger logger);
	}
}
