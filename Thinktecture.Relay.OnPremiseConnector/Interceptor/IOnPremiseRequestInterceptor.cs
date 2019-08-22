namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	public interface IOnPremiseRequestInterceptor
	{
		void HandleRequest(IInterceptedRequest request);
	}
}
