using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Connector;
using Thinktecture.Relay.OnPremiseConnector.SignalR;

namespace Thinktecture.Relay.OnPremiseConnector.NewServerSupport
{
	internal class NewServerConnection : IRelayServerConnection
	{
		internal static string _relayedRequestHeader;

		private readonly IConnectorConnection _connection;

		/// <inheritdoc />
		public String RelayedRequestHeader
		{
			get => _relayedRequestHeader;
			set => _relayedRequestHeader = value;
		}

		public Int32 RelayServerConnectionInstanceId => RelayServerSignalRConnection._nextInstanceId;

		public event EventHandler Disposing;
		public event EventHandler Connected;
		public event EventHandler Disconnected;
		public event EventHandler Reconnecting;
		public event EventHandler Reconnected;

		public NewServerConnection(ILogger<NewServerConnection> logger, IConnectorConnection connection)
		{
			logger.LogInformation("Creating new v3 connection for RelayServer");
			_connection = connection ?? throw new ArgumentNullException(nameof(connection));

			// TODO: Subscribe to events when they become available
		}

		public void RegisterOnPremiseTarget(String key, Uri baseUri, Boolean followRedirects)
		{
			// TODO: Add registering when it becomes available
			throw new NotImplementedException();
		}

		public void RemoveOnPremiseTarget(String key)
		{
			// TODO: Add registering when it becomes available
			throw new NotImplementedException();
		}

		// Need to Implement
		public Task SendAcknowledgmentAsync(Guid acknowledgeOriginId, String acknowledgeId, String connectionId = null)
		{
			// TODO: Add support for acknowledgement when it becomes available
			throw new NotImplementedException();
		}

		public async Task ConnectAsync()
		{
			 await _connection.ConnectAsync();
			 Connected?.Invoke(this, EventArgs.Empty);
		}

		public void Disconnect() => DisconnectAsync().GetAwaiter().GetResult();

		private async Task DisconnectAsync()
		{
			await _connection.DisconnectAsync();
			Disconnected?.Invoke(this, EventArgs.Empty);
		}


		#region Not supported stuff used by maintenance loop

		// Used by MaintenanceLoop -> TokenExpiryChecker
		// Maintenance Loop is NOT started on new relay server v3 connections
		public DateTime TokenExpiry =>
			throw new NotSupportedException();

		public TimeSpan TokenRefreshWindow =>
			throw new NotSupportedException();

		public Task<Boolean> TryRequestAuthorizationTokenAsync() =>
			throw new NotSupportedException();

		// Used by MaintenanceLoop -> HeartbeatChecker
		// Maintenance Loop is NOT started on new relay server v3 connections
		public DateTime LastHeartbeat =>
			throw new NotSupportedException();

		public TimeSpan HeartbeatInterval =>
			throw new NotSupportedException();

		// Used by MaintenanceLoop -> AutomaticDisconnectChecker
		// Maintenance Loop is NOT started on new relay server v3 connections
		public DateTime? ConnectedSince =>
			throw new NotSupportedException();

		public DateTime? LastActivity =>
			throw new NotSupportedException();

		public TimeSpan? AbsoluteConnectionLifetime =>
			throw new NotSupportedException();

		public TimeSpan? SlidingConnectionLifetime =>
			throw new NotSupportedException();

		// Only used by maintenance checkers on the old connection.
		public Uri Uri =>
			throw new NotSupportedException();

		public void Reconnect() =>
			throw new NotSupportedException();

		#endregion

		#region not supported methods for InProc targets

		// No support of In-Proc targets in 2.5er connector, as this is not used as far as we know
		public void RegisterOnPremiseTarget(String key, Type handlerType) =>
			throw new NotSupportedException();

		public void RegisterOnPremiseTarget(String key, Func<IOnPremiseInProcHandler> handlerFactory) =>
			throw new NotSupportedException();

		public void RegisterOnPremiseTarget<T>(String key) where T : IOnPremiseInProcHandler, new() =>
			throw new NotSupportedException();

		// In-Proc Support, not used as far as we know as of 2020-11-27
		public Task<HttpResponseMessage> GetToRelayAsync(String relativeUrl, Action<HttpRequestHeaders> setHeaders, CancellationToken cancellationToken) =>
			throw new NotSupportedException();

		public Task<HttpResponseMessage> PostToRelayAsync(String relativeUrl, Action<HttpRequestHeaders> setHeaders, HttpContent content, CancellationToken cancellationToken) =>
			throw new NotSupportedException();

		#endregion

		// Dynamic Target registration, not used
		public List<String> GetOnPremiseTargetKeys() => throw new NotSupportedException();

		#region disposing pattern

		protected virtual void OnDisposing()
		{
			Disposing?.Invoke(this, EventArgs.Empty);
		}

		protected void Dispose(bool disposing)
		{
			OnDisposing();

			if (disposing)
			{
				(_connection as IDisposable)?.Dispose();
				(_connection as IAsyncDisposable)?.DisposeAsync().GetAwaiter().GetResult();
			}
		}

		public void Dispose() => Dispose(true);

		#endregion
	}
}
