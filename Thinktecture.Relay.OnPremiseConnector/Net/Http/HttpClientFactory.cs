using System;
using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	///<inheritdoc/>
	public class HttpClientFactory : IHttpClientFactory
	{
		///<inheritdoc/>
		public HttpClient CreateClient(String name)
		{
			if (name == "RedirectableWebTarget")
				return new HttpClient(new HttpClientHandler() { AllowAutoRedirect = true});
			return new HttpClient();
		}
	}
}
