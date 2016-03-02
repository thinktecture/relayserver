namespace Thinktecture.Relay.Server.Communication
{
	internal interface IOnPremiseConnectorCallbackFactory
	{
		IOnPremiseConnectorCallback Create(string requestId);
	}
}