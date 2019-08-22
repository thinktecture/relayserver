namespace Thinktecture.Relay.OnPremiseConnector.Interceptor
{
	internal interface IOnPremiseInterceptorFactory
	{
		IOnPremiseRequestInterceptor CreateOnPremiseRequestInterceptor();
		IOnPremiseResponseInterceptor CreateOnPremiseResponseInterceptor();
	}
}
