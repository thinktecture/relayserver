using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Thinktecture.Relay.OnPremiseConnector.SignalR
{
	internal interface IRelayServerConnection : IDisposable
	{
		void RegisterOnPremiseTarget(string key, Uri baseUri);
		void RegisterOnPremiseTarget(string key, Type handlerType);
		void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory);
		void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new();
		void RemoveOnPremiseTarget(string key);
		string RelayedRequestHeader { get; set; }
		Task ConnectAsync();
		void Disconnect();
		List<string> GetOnPremiseTargetKeys();

		Task<HttpResponseMessage> GetToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken);
		Task<HttpResponseMessage> PostToRelay(string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken);
	}
}
