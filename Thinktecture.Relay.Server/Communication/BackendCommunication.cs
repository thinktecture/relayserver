using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;
using Thinktecture.Relay.Server.Repository;

namespace Thinktecture.Relay.Server.Communication
{
	internal class BackendCommunication : IBackendCommunication, IDisposable
	{
		private readonly CancellationTokenSource _cts;
		private readonly CancellationToken _cancellationToken;
		private readonly IConfiguration _configuration;
		private readonly IMessageDispatcher _messageDispatcher;
		private readonly IOnPremiseConnectorCallbackFactory _requestCallbackFactory;
		private readonly ILogger _logger;
		private readonly ILinkRepository _linkRepository;

		private readonly ConcurrentDictionary<string, IOnPremiseConnectorCallback> _requestCompletedCallbacks;
		private readonly ConcurrentDictionary<string, IOnPremiseConnectionContext> _connectionContexts;
		private readonly TimeSpan _heartbeatInterval;

		private readonly Dictionary<string, IDisposable> _requestSubscriptions;
		private IDisposable _responseSubscription;

		public Guid OriginId { get; }

		public BackendCommunication(IConfiguration configuration, IMessageDispatcher messageDispatcher, IOnPremiseConnectorCallbackFactory requestCallbackFactory, ILogger logger, IPersistedSettings persistedSettings, ILinkRepository linkRepository)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
			_requestCallbackFactory = requestCallbackFactory ?? throw new ArgumentNullException(nameof(requestCallbackFactory));
			_logger = logger;
			_linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));
			_requestCompletedCallbacks = new ConcurrentDictionary<string, IOnPremiseConnectorCallback>(StringComparer.OrdinalIgnoreCase);
			_connectionContexts = new ConcurrentDictionary<string, IOnPremiseConnectionContext>();
			_heartbeatInterval = new TimeSpan(_configuration.ActiveConnectionTimeout.Ticks / 2);
			_requestSubscriptions = new Dictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
			_cts = new CancellationTokenSource();
			_cancellationToken = _cts.Token;
			OriginId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));

			_logger?.Verbose("Creating backend communication. origin-id={OriginId}", OriginId);
			_logger?.Information("Backend communication is using message dispatcher {MessageDispatcherType}", messageDispatcher.GetType().Name);
		}

		public void Prepare()
		{
			_linkRepository.DeleteAllConnectionsForOrigin(OriginId);
			_responseSubscription = StartReceivingResponses(OriginId);

			StartSendHeartbeatsLoop(_cts.Token);
		}

		public Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId)
		{
			return GetResponseAsync(requestId, _configuration.OnPremiseConnectorCallbackTimeout);
		}

		private void CheckDisposed()
		{
			if (_cts.IsCancellationRequested)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		private Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId, TimeSpan requestTimeout)
		{
			CheckDisposed();
			_logger?.Debug("Waiting for response. request-id={RequestId}", requestId);

			var onPremiseConnectorCallback = _requestCompletedCallbacks[requestId] = _requestCallbackFactory.Create(requestId);

			return GetOnPremiseTargetResponseAsync(onPremiseConnectorCallback, requestTimeout, _cancellationToken);
		}

		private async Task<IOnPremiseConnectorResponse> GetOnPremiseTargetResponseAsync(IOnPremiseConnectorCallback callback, TimeSpan requestTimeout, CancellationToken cancellationToken)
		{
			try
			{
				using (var timeoutCts = new CancellationTokenSource(requestTimeout))
				using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
				{
					var token = cts.Token;

					using (token.Register(() => callback.Response.TrySetCanceled(token)))
					{
						var response = await callback.Response.Task.ConfigureAwait(false);
						_logger?.Debug("Received on-premise response. request-id={RequestId}", callback.RequestId);

						return response;
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger?.Debug("No response received within specified timeout. callback-timeout={CallbackTimout}, request-id={RequestId}", _configuration.OnPremiseConnectorCallbackTimeout, callback.RequestId);
			}
			catch (Exception ex)
			{
				_logger?.Debug(ex, "Error during waiting for on-premise connector response. request-id={RequestId}", callback.RequestId);
			}
			finally
			{
				_requestCompletedCallbacks.TryRemove(callback.RequestId, out var removed);
			}

			return null;
		}

		public async Task SendOnPremiseConnectorRequestAsync(Guid linkId, IOnPremiseConnectorRequest request)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching request. request-id={RequestId}, link-id={LinkId}", request.RequestId, linkId);

			await _messageDispatcher.DispatchRequest(linkId, request).ConfigureAwait(false);
		}

		public void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId)
		{
			CheckDisposed();

			_messageDispatcher.AcknowledgeRequest(acknowledgeId);

			if (connectionId == null)
				return;

				_linkRepository.RenewActiveConnectionAsync(connectionId);
			if (_connectionContexts.TryGetValue(connectionId, out var connectionContext))
				connectionContext.LastLocalActivity = DateTime.UtcNow;
		}

		public async Task RegisterOnPremiseAsync(IOnPremiseConnectionContext onPremiseConnectionContext)
		{
			CheckDisposed();

			_logger?.Debug("Registering link. link-id={LinkId}, connection-id={ConnectionId}, user-name={UserName}, role={Role} connector-version={ConnectorVersion}, connector-assembly-version={ConnectorAssemblyVersion}",
				onPremiseConnectionContext.LinkId,
				onPremiseConnectionContext.ConnectionId,
				onPremiseConnectionContext.UserName,
				onPremiseConnectionContext.Role,
				onPremiseConnectionContext.ConnectorVersion,
				onPremiseConnectionContext.ConnectorAssemblyVersion);

			await _linkRepository.AddOrRenewActiveConnectionAsync(onPremiseConnectionContext.LinkId, OriginId, onPremiseConnectionContext.ConnectionId, onPremiseConnectionContext.ConnectorVersion, onPremiseConnectionContext.ConnectorAssemblyVersion).ConfigureAwait(false);

			lock (_requestSubscriptions)
			{
				if (!_requestSubscriptions.ContainsKey(onPremiseConnectionContext.ConnectionId))
				{
					_requestSubscriptions[onPremiseConnectionContext.ConnectionId] = _messageDispatcher.OnRequestReceived(onPremiseConnectionContext.LinkId, onPremiseConnectionContext.ConnectionId, !onPremiseConnectionContext.SupportsAck)
						.Subscribe(request => onPremiseConnectionContext.RequestAction(request, _cancellationToken));
				}
			}

			_connectionContexts.TryAdd(onPremiseConnectionContext.ConnectionId, onPremiseConnectionContext);
		}

		public async Task UnregisterOnPremiseAsync(string connectionId)
		{
			CheckDisposed();

			await UnsubscribeConnectionAsync(connectionId);
			UnregisterForHeartbeat(connectionId);
		}

		public async Task SendOnPremiseTargetResponseAsync(Guid originId, IOnPremiseConnectorResponse response)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching response. origin-id={OriginId}", originId);

			await _messageDispatcher.DispatchResponse(originId, response).ConfigureAwait(false);
		}

		private IDisposable StartReceivingResponses(Guid originId)
		{
			_logger?.Debug("Start receiving responses from dispatcher. origin-id={OriginId}", originId);

			return _messageDispatcher.OnResponseReceived(originId).Subscribe(ForwardOnPremiseTargetResponse);
		}

		private void ForwardOnPremiseTargetResponse(IOnPremiseConnectorResponse response)
		{
			if (_requestCompletedCallbacks.TryRemove(response.RequestId, out var onPremiseConnectorCallback))
			{
				_logger?.Debug("Forwarding on-premise target response. request-id={RequestId}", response.RequestId);
				onPremiseConnectorCallback.Response.SetResult(response);
			}
			else
			{
				_logger?.Debug("Response received but no request callback found. request-id={RequestId}", response.RequestId);
			}
		}

		private async Task UnsubscribeConnectionAsync(string connectionId)
		{
			_connectionContexts.TryGetValue(connectionId, out var connectionInfo);
			_logger?.Debug("Unregistering on-premise link. link-id={LinkId}, connection-id={ConnectionId}", connectionInfo?.LinkId, connectionId);

			await _linkRepository.RemoveActiveConnectionAsync(connectionId).ConfigureAwait(false);

			IDisposable requestSubscription;
			lock (_requestSubscriptions)
			{
				if (_requestSubscriptions.TryGetValue(connectionId, out requestSubscription))
					_requestSubscriptions.Remove(connectionId);
			}

			if (requestSubscription != null)
			{
				_logger?.Debug("Disposing request subscription. link-id={LinkId}, connection-id={ConnectionId}", connectionInfo?.LinkId, connectionId);
				requestSubscription.Dispose();
			}
		}

		private void UnregisterForHeartbeat(string connectionId)
		{
			_logger?.Verbose("Unregistering from heartbeating. connection-id={ConnectionId}", connectionId);

			_connectionContexts.TryRemove(connectionId, out var info);
		}

		private void StartSendHeartbeatsLoop(CancellationToken token)
		{
			Task.Run(async () =>
			{
				while (!token.IsCancellationRequested)
				{
					await Task.WhenAll(_connectionContexts.Values.Select(heartbeatClient => SendHeartbeatAsync(heartbeatClient, token))).ConfigureAwait(false);
					await Task.Delay(_heartbeatInterval, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private async Task SendHeartbeatAsync(IOnPremiseConnectionContext connectionContext, CancellationToken token)
		{
			if (connectionContext == null)
				throw new ArgumentNullException(nameof(connectionContext));

			if (!connectionContext.SupportsHeartbeat)
				return;

			try
			{
				_logger?.Verbose("Sending heartbeat. connection-id={ConnectionId}", connectionContext.ConnectionId);

				var requestId = Guid.NewGuid().ToString();
				var request = new OnPremiseConnectorRequest()
				{
					HttpMethod = "HEARTBEAT",
					Url = String.Empty,
					RequestStarted = DateTime.UtcNow,
					OriginId = OriginId,
					RequestId = requestId,
					AcknowledgeId = requestId,
					HttpHeaders = new Dictionary<string, string> { ["X-TTRELAY-HEARTBEATINTERVAL"] = _heartbeatInterval.TotalSeconds.ToString(CultureInfo.InvariantCulture) },
				};

				// heartbeats do NOT go through the message dispatcher as we want to heartbeat the connections directly
				await connectionContext.RequestAction(request, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during sending heartbeat to a client. link-id={LinkId}, connection-id={ConnectionId}, connector-version={ConnectorVersion}", connectionContext.LinkId, connectionContext.ConnectionId, connectionContext.ConnectorVersion);
			}
		}

		#region IDisposable

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				_cts.Cancel();
				_cts.Dispose();

				List<IDisposable> subscriptions;

				lock (_requestSubscriptions)
				{
					subscriptions = _requestSubscriptions.Values.ToList();
				}

				foreach (var subscription in subscriptions)
				{
					subscription.Dispose();
				}

				_responseSubscription?.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
		}

		#endregion
	}
}
