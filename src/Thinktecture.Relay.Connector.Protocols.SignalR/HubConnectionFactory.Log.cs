using Microsoft.Extensions.Logging;

namespace Thinktecture.Relay.Connector.Protocols.SignalR;

public partial class HubConnectionFactory
{
	private static partial class Log
	{
		[LoggerMessage(LoggerEventIds.HubConnectionFactoryCreatingConnection, LogLevel.Information,
			"Creating connection to {ConnectorEndpoint}")]
		public static partial void CreatingConnection(ILogger logger, string connectorEndpoint);
	}
}
