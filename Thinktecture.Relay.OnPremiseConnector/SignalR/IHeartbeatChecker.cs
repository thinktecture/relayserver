namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IHeartbeatChecker
	{
		void CheckHeartbeat(IRelayServerConnection connection);
	}
}
