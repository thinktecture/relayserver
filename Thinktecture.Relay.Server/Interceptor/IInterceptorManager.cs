using System;
using System.Net.Http;
using System.Security.Principal;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Interceptor
{
	public interface IInterceptorManager
	{
		IOnPremiseConnectorRequest HandleRequest(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, out HttpResponseMessage immediateResponse);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, HttpRequestMessage message, IPrincipal clientUser, IOnPremiseConnectorResponse response);
	}
}
