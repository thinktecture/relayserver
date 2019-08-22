namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	public interface IOnPremiseResponseInterceptor
	{
		void HandleResponse(IInterceptedRequest request, IInterceptedResponse response);
	}
}
