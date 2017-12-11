using System;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerConnectionFactory
	{
		IRelayServerConnection Create(string userName, string password, Uri relayServer, int requestTimeout);
	}
}
