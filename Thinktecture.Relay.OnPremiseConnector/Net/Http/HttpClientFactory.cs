using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	///<inheritdoc/>
	public class HttpClientFactory : IHttpClientFactory
	{
		///<inheritdoc/>
		public HttpClient CreateClient(string name)
		{
			// TODO: Adjust name to the new one from v3
			return (name == "FollowRedirectsWebTarget")
				? new HttpClient(new HttpClientHandler() { UseCookies = false, })
				: new HttpClient(new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false, });
		}
	}
}
