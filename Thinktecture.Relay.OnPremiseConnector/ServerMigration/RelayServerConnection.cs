using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.Connector.Targets;
using Thinktecture.Relay.OnPremiseConnector.SignalR;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.OnPremiseConnector.ServerMigration
{
	internal class RelayServerConnection : IRelayServerConnection
	{
		private static ConcurrentDictionary<string, (Uri Uri, bool FollowRedirects)> _registeredTargets = new ConcurrentDictionary<string, (Uri, bool)>();

		private readonly IConnectorConnection _connection;
		private readonly RelayTargetRegistry<ClientRequest, TargetResponse> _targetRegistry;
		private readonly IHttpClientFactory _httpClientFactory;

		public string RelayedRequestHeader => RelayServerConnector.GetRelayedRequestHeader();
		public int RelayServerConnectionInstanceId { get; } = RelayServerConnector.GetNextInstanceId();

		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler Disposing;
		public event EventHandler Reconnecting;
		public event EventHandler Reconnected;

		public RelayServerConnection(ILogger<RelayServerConnection> logger, IConnectorConnection connection, RelayTargetRegistry<ClientRequest, TargetResponse> targetRegistry, IHttpClientFactory httpClientFactory)
		{
			logger.LogInformation("Creating v3 connection for RelayServer");

			_connection = connection ?? throw new ArgumentNullException(nameof(connection));
			_targetRegistry = targetRegistry ?? throw new ArgumentNullException(nameof(targetRegistry));
			_httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));

			foreach (var registration in _registeredTargets)
			{
				_targetRegistry.Unregister(registration.Key);
				_targetRegistry.Register(registration.Key, typeof(WebTarget), null, registration.Value.Uri, registration.Value.FollowRedirects ? RelayWebTargetOptions.FollowRedirect : RelayWebTargetOptions.None);
			}

			_connection.Connected += ConnectionConnected;
			_connection.Disconnected += ConnectionDisconnected;
			_connection.Reconnecting += ConnectionReconnecting;
			_connection.Reconnected += ConnectionReconnected;
		}

		private Task ConnectionConnected(object sender, string connectionId)
		{
			Connected?.Invoke(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		private Task ConnectionDisconnected(object sender, string connectionId)
		{
			Disconnected?.Invoke(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		private Task ConnectionReconnecting(object sender, string connectionId)
		{
			Reconnecting?.Invoke(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		private Task ConnectionReconnected(object sender, string @event)
		{
			Reconnected?.Invoke(this, EventArgs.Empty);
			return Task.CompletedTask;
		}

		public void RegisterOnPremiseTarget(string key, Uri baseUri, bool followRedirects) => _targetRegistry.Register(key, typeof(WebTarget), null, baseUri, followRedirects ? RelayWebTargetOptions.FollowRedirect : RelayWebTargetOptions.None);

		public static void RegisterStaticOnPremiseTarget(string key, Uri baseUri, bool followRedirects) => _registeredTargets.TryAdd(key, (baseUri, followRedirects));

		public void RemoveOnPremiseTarget(string key) => _targetRegistry.Unregister(key);

		public static void RemoveStaticOnPremiseTarget(string key) => _registeredTargets.TryRemove(key, out _);

		public async Task SendAcknowledgmentAsync(Guid acknowledgeOriginId, string requestId, string connectionId = null)
		{
			using (var client = _httpClientFactory.CreateClient(Constants.HttpClientNames.RelayServer))
			{
				await client.PostAsync($"acknowledge/{acknowledgeOriginId}/{requestId}", new StringContent(string.Empty)).ConfigureAwait(false);
			}
		}

		public async Task ConnectAsync() => await _connection.ConnectAsync().ConfigureAwait(false);

		public void Disconnect() => _connection.DisconnectAsync().GetAwaiter().GetResult();

		#region Not supported stuff used by maintenance loop

		// Used by MaintenanceLoop -> TokenExpiryChecker
		// Maintenance Loop is NOT started on relay server v3 connections
		public DateTime TokenExpiry => throw new NotSupportedException();

		public TimeSpan TokenRefreshWindow => throw new NotSupportedException();

		public Task<bool> TryRequestAuthorizationTokenAsync() => throw new NotSupportedException();

		// Used by MaintenanceLoop -> HeartbeatChecker
		// Maintenance Loop is NOT started on relay server v3 connections
		public DateTime LastHeartbeat => throw new NotSupportedException();

		public TimeSpan HeartbeatInterval => throw new NotSupportedException();

		// Used by MaintenanceLoop -> AutomaticDisconnectChecker
		// Maintenance Loop is NOT started on relay server v3 connections
		public DateTime? ConnectedSince => throw new NotSupportedException();

		public DateTime? LastActivity => throw new NotSupportedException();

		public TimeSpan? AbsoluteConnectionLifetime => throw new NotSupportedException();

		public TimeSpan? SlidingConnectionLifetime => throw new NotSupportedException();

		// Only used by maintenance checkers on the old connection.
		public Uri Uri => throw new NotSupportedException();

		public void Reconnect() => throw new NotSupportedException();

		#endregion

		#region Not supported methods for In-Proc targets

		// No support of In-Proc targets, as this is not used as far as we know
		public void RegisterOnPremiseTarget(string key, Type handlerType) => throw new NotSupportedException();

		public void RegisterOnPremiseTarget(string key, Func<IOnPremiseInProcHandler> handlerFactory) => throw new NotSupportedException();

		public void RegisterOnPremiseTarget<T>(string key) where T : IOnPremiseInProcHandler, new() => throw new NotSupportedException();

		public Task<HttpResponseMessage> GetToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken) => throw new NotSupportedException();

		public Task<HttpResponseMessage> PostToRelayAsync(string relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken) => throw new NotSupportedException();

		#endregion

		// Registered keys are retrieved from v2 connection 
		public List<string> GetOnPremiseTargetKeys() => throw new NotSupportedException();

		#region Disposing pattern

		protected void OnDisposing() => Disposing?.Invoke(this, EventArgs.Empty);

		protected void Dispose(bool disposing)
		{
			OnDisposing();

			if (disposing)
			{
				_connection.Connected -= ConnectionConnected;
				_connection.Disconnected -= ConnectionDisconnected;
				_connection.Reconnecting -= ConnectionReconnecting;
				_connection.Reconnected -= ConnectionReconnected;

				(_connection as IDisposable)?.Dispose();
				(_connection as IAsyncDisposable)?.DisposeAsync().GetAwaiter().GetResult();
			}
		}

		public void Dispose() => Dispose(true);

		#endregion
	}
}
