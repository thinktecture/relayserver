using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	///<inheritdoc/>
	public class HttpClientFactory : IHttpClientFactory
	{
		///<inheritdoc/>
		public HttpClient CreateClient(string name)
		{
			return (name == "FollowRedirectsWebTarget")
				? new HttpClient(new HttpClientHandler() { UseCookies = false, })
				: new HttpClient(new HttpClientHandler() { UseCookies = false, AllowAutoRedirect = false, });
		}
	}
}
