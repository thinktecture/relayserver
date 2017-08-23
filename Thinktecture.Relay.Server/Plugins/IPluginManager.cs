using System.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	internal interface IPluginManager
	{
		HttpResponseMessage HandleRequest(OnPremiseConnectorRequest onPremiseConnectorRequest);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest onPremiseConnectorRequest, OnPremiseTargetResponse onPremiseTargetResponse);
	}
}
