using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	public interface IHttpClientFactory
	{
		HttpClient CreateClient(string name);
	}
}
