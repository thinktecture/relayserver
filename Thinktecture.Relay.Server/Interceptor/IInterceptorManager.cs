using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IInterceptorManager
	{
		HttpResponseMessage HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response);
	}
}
