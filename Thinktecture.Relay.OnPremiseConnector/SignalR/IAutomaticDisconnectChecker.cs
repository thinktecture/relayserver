namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IAutomaticDisconnectChecker
	{
		bool DisconnectIfRequired(IRelayServerConnection connection);
	}
}
