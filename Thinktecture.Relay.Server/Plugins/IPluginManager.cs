using System.Net.Http;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Plugins
{
	public interface IPluginManager
	{
		HttpResponseMessage HandleRequest(IOnPremiseConnectorRequest request);
		HttpResponseMessage HandleResponse(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response);
	}
}
