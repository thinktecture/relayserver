namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IHeartbeatChecker
	{
		void Check(IRelayServerConnection connection);
	}
}
