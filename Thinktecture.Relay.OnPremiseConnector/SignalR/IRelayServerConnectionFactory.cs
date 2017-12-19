using System;
using System.Reflection;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerConnectionFactory
	{
		IRelayServerConnection Create(string userName, string password, Uri relayServer, int requestTimeoutInSeconds, int tokenRefreshWindowInSeconds);
	}
}
