namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerConnectionFactory
	{
		IRelayServerConnection Create(RelayServerConnectionConfig config);
	}
}
