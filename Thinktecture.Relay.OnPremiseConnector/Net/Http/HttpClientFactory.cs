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
			// We ignore the name here, as this should be as simple as it gets.
			return new HttpClient();
		}
	}
}
