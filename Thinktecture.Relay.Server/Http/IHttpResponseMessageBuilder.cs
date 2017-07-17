using System.Net.Http;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Http
{
	public interface IHttpResponseMessageBuilder
	{
		HttpResponseMessage BuildFrom(IOnPremiseTargetResponse onPremiseTargetResponse, Link link);
	}
}