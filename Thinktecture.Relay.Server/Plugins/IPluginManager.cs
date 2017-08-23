using System.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	internal interface IPluginManager
	{
		OnPremiseConnectorRequest HandleRequest(OnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response);
		OnPremiseTargetResponse HandleResponse(OnPremiseTargetResponse onPremiseTargetResponse, IOnPremiseConnectorRequest onPremiseConnectorRequest, out HttpResponseMessage response);
	}
}
