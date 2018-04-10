using System;
using System.Security.Authentication;
using System.Threading.Tasks;
using Serilog;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal class TokenExpiryChecker : ITokenExpiryChecker
	{
		private readonly ILogger _logger;

		public TokenExpiryChecker(ILogger logger)
		{
			_logger = logger;
		}

		public async Task Check(IRelayServerConnection connection)
		{
			if (connection == null)
				throw new ArgumentNullException(nameof(connection));

			var logger = _logger?
				.ForContext("RelayServerUri", connection.Uri)
				.ForContext("RelayServerConnectionInstanceId", connection.RelayServerConnectionInstanceId);

			if ((connection.TokenExpiry - connection.TokenRefreshWindow) <= DateTime.UtcNow)
			{
				logger?.Information("Access token is going to expire soon. Trying to refresh token for RelayServer {RelayServerUri} with connection instance id {RelayServerConnectionInstanceId}");

				try
				{
					if (!await connection.TryRequestAuthorizationTokenAsync().ConfigureAwait(false))
						throw new AuthenticationException();
				}
				catch (AuthenticationException)
				{
					logger?.Warning("Could not renew access token and trying a hard reconnect. relay-server={RelayServerUri}, relay-server-connection-instance-id={RelayServerConnectionInstanceId}");

					connection.Reconnect();
				}
			}
		}
	}
}
