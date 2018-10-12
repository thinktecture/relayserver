using System;
using System.Net.Http;

namespace Thinktecture.Relay.OnPremiseConnector.Net.Http
{
	public class HttpClientFactory : IHttpClientFactory
	{
		public HttpClient CreateClient(String name)
		{
			// We ignore the name here, as this should be as simple as it gets.
			return new HttpClient();
		}
	}
}
