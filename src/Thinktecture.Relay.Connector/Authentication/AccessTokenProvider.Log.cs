using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Authentication;

internal partial class AccessTokenProvider
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.AccessTokenProviderRequestingAccessToken, LogLevel.Error, "Requesting access token")]
		public static partial void RequestingAccessToken(ILogger logger);
	}
}
