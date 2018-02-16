using System;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IMaintenanceLoop : IDisposable
	{
		void RegisterConnection(IRelayServerConnection connection);
		void UnregisterConnection(IRelayServerConnection connection);
	}
}
