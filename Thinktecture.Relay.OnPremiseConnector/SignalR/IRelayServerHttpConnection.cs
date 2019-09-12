using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerHttpConnection : IDisposable
	{
		Task<HttpResponseMessage> SendToRelayAsync(string relativeUrl, HttpMethod httpMethod, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken);
		void SetBearerToken(string accessToken);
	}
}
