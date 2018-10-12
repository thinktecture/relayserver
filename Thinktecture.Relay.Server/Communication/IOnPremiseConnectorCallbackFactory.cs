namespace Thinktecture.Relay.Server.Communication
{
	public interface IOnPremiseConnectorCallbackFactory
	{
		IOnPremiseConnectorCallback Create(string requestId);
	}
}
