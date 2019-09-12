using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	public interface IHttpResponseMessageBuilder
	{
		HttpResponseMessage BuildFromConnectorResponse(IOnPremiseConnectorResponse response, bool forwardOnPremiseTargetErrorResponse, string requestId);
	}
}
