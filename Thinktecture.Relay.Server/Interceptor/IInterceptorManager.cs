using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IInterceptorManager
	{
		IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, out HttpResponseMessage immediateResponse);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, HttpRequestMessage message, IOnPremiseConnectorResponse response);
	}
}
