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
		string RelayedRequestHeader { get; set; }
		Uri Uri { get; }
		TimeSpan TokenRefreshWindow { get; }
		DateTime TokenExpiry { get; }
		int RelayServerConnectionInstanceId { get; }

		DateTime? ConnectedSince { get; }
		DateTime? LastActivity { get; }
		TimeSpan? AbsoluteConnectionLifetime { get; }
		TimeSpan? SlidingConnectionLifetime { get; }

		event EventHandler Disposing;
		event EventHandler Connected;
		event EventHandler Disconnected;

		void RegisterOnPremiseTarget(string key, Uri baseUri, bool followRedirects);
		void RegisterOnPremiseTarget(string key, Type handlerType);
		void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory);
		void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new();
		void RemoveOnPremiseTarget(string key);
		Task ConnectAsync();
		void Disconnect();
		void Reconnect();
		Task<bool> TryRequestAuthorizationTokenAsync();

		List<string> GetOnPremiseTargetKeys();
		Task<HttpResponseMessage> GetToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken);
		Task<HttpResponseMessage> PostToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken);
		Task SendAcknowledgmentAsync(Guid acknowledgeOriginId, string acknowledgeId, string connectionId = null);

		void CheckHeartbeat();
	}
}
