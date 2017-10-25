using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptors
{
	public interface IInterceptorManager
	{
		HttpResponseMessage HandleRequest(IOnPremiseConnectorRequest request);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response);
	}
}
