using System;
using System.Net.Http;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Http
{
	public interface IOnPremiseRequestBuilder
	{
		Task<IOnPremiseConnectorRequest> BuildFromHttpRequest(HttpRequestMessage message, Guid originId, string pathWithoutUserName, string basePath);
	}
}
