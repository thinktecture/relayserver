using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Serilog;
using Thinktecture.Relay.Server.Communication.RabbitMq;
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
		private readonly TimeSpan _lastActivityDbUpdateDelay;

		private readonly ConcurrentDictionary<string, IOnPremiseConnectorCallback> _requestCompletedCallbacks;
		private readonly ConcurrentDictionary<string, IOnPremiseConnectionContext> _connectionContexts;

		private readonly Dictionary<string, IDisposable> _requestSubscriptions;

		private IDisposable _responseSubscription;
		private IDisposable _acknowledgeSubscription;

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
			_requestSubscriptions = new Dictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
			_cts = new CancellationTokenSource();
			_cancellationToken = _cts.Token;
			_lastActivityDbUpdateDelay = new TimeSpan(_configuration.ActiveConnectionTimeout.Ticks / 5);
			OriginId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));

			_logger?.Verbose("Creating backend communication. origin-id={OriginId}", OriginId);
			_logger?.Information("Backend communication is using message dispatcher {MessageDispatcherType}", messageDispatcher.GetType().Name);
		}

		public void Prepare()
		{
			_linkRepository.DeleteAllConnectionsForOrigin(OriginId);
			_responseSubscription = StartReceivingResponses();
			_acknowledgeSubscription = StartReceivingAcknowledges();
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

					onPremiseConnectionContext.IsActive = true;
				}
			}

			_connectionContexts.TryAdd(onPremiseConnectionContext.ConnectionId, onPremiseConnectionContext);

			if (onPremiseConnectionContext.SupportsConfiguration)
			{
				await ProvideLinkConfigurationAsync(onPremiseConnectionContext).ConfigureAwait(false);
			}
		}

		public async Task UnregisterOnPremiseConnectionAsync(string connectionId)
		{
			CheckDisposed();

			await DeactivateOnPremiseConnectionAsync(connectionId);

			_logger?.Verbose("Unregistering connection. connection-id={ConnectionId}", connectionId);
			_connectionContexts.TryRemove(connectionId, out var info);
		}

		public async Task DeactivateOnPremiseConnectionAsync(string connectionId)
		{
			if (_connectionContexts.TryGetValue(connectionId, out var connectionContext))
			{
				connectionContext.IsActive = false;
			}

			_logger?.Debug("Deactivating connection. link-id={LinkId}, connection-id={ConnectionId}", connectionContext?.LinkId, connectionId);

			await _linkRepository.RemoveActiveConnectionAsync(connectionId).ConfigureAwait(false);

			IDisposable requestSubscription;
			lock (_requestSubscriptions)
			{
				if (_requestSubscriptions.TryGetValue(connectionId, out requestSubscription))
					_requestSubscriptions.Remove(connectionId);
			}

			if (requestSubscription != null)
			{
				_logger?.Debug("Disposing request subscription. link-id={LinkId}, connection-id={ConnectionId}", connectionContext?.LinkId, connectionId);
				requestSubscription.Dispose();
			}
		}

		public Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId, TimeSpan? requestTimeout = null)
		{
			CheckDisposed();

			var timeout = requestTimeout ?? _configuration.OnPremiseConnectorCallbackTimeout;

			_logger?.Debug("Waiting for response. request-id={RequestId}, timeout={Timeout}", requestId, timeout);

			var onPremiseConnectorCallback = _requestCompletedCallbacks[requestId] = _requestCallbackFactory.Create(requestId);

			return GetOnPremiseTargetResponseAsync(onPremiseConnectorCallback, timeout, _cancellationToken);
		}

		public void SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching request. request-id={RequestId}, link-id={LinkId}", request.RequestId, linkId);

			_messageDispatcher.DispatchRequest(linkId, request);
		}

		public async Task AcknowledgeOnPremiseConnectorRequestAsync(Guid originId, string connectionId, string acknowledgeId)
		{
			CheckDisposed();

			if (originId != Guid.Empty)
			{
				_logger?.Debug("Dispatching acknowledge. origin-id={OriginId}, connection-id={ConnectionId}, acknowledge-id={AcknowledgeId}", originId, connectionId, acknowledgeId);
				_messageDispatcher.DispatchAcknowledge(originId, connectionId, acknowledgeId);
			}

			if (connectionId != null)
			{
				await RenewLastActivityAsync(connectionId).ConfigureAwait(false);
			}
		}

		public async Task RenewLastActivityAsync(string connectionId)
		{
			var now = DateTime.UtcNow;

			if (_connectionContexts.TryGetValue(connectionId, out var connectionContext))
			{
				connectionContext.LastLocalActivity = now;

				if (!connectionContext.IsActive)
				{
					await RegisterOnPremiseAsync(connectionContext);
				}

				if (connectionContext.LastDbActivity + _lastActivityDbUpdateDelay < now)
				{
					connectionContext.LastDbActivity = now;
					await _linkRepository.RenewActiveConnectionAsync(connectionId);
				}
			}
			else
			{
				await _linkRepository.RenewActiveConnectionAsync(connectionId);
			}
		}

		public void SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching response. origin-id={OriginId}", originId);

			_messageDispatcher.DispatchResponse(originId, response);
		}

		public IEnumerable<IOnPremiseConnectionContext> GetConnectionContexts()
		{
			return _connectionContexts.Values;
		}

		private async Task ProvideLinkConfigurationAsync(IOnPremiseConnectionContext onPremiseConnectionContext)
		{
			try
			{
				var config = _linkRepository.GetLinkConfiguration(onPremiseConnectionContext.LinkId);
				config.ApplyDefaults(_configuration);

				var requestId = Guid.NewGuid().ToString();

				_logger?.Debug("Sending configuration to OPC. connection-id={ConnectionId}, link-id={LinkId}, connector-version={ConnectorVersion}, link-configuration={@LinkConfiguration}, request-id={RequestId}", onPremiseConnectionContext.ConnectionId, onPremiseConnectionContext.LinkId, onPremiseConnectionContext.ConnectorVersion, config, requestId);

				var request = new OnPremiseConnectorRequest()
				{
					HttpMethod = "CONFIG",
					Url = String.Empty,
					RequestStarted = DateTime.UtcNow,
					OriginId = OriginId,
					RequestId = requestId,
					AcknowledgmentMode = AcknowledgmentMode.Auto,
					Body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(config)),
				};

				// configs, like heartbeats, do not go through the message dispatcher but directly to the connection
				await onPremiseConnectionContext.RequestAction(request, CancellationToken.None).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "An error happened while sending the link config to the connected OPC.");
			}
		}

		private IDisposable StartReceivingResponses()
		{
			_logger?.Debug("Start receiving responses from dispatcher. origin-id={OriginId}", OriginId);

			return _messageDispatcher.OnResponseReceived().Subscribe(ForwardOnPremiseTargetResponse);
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
				_logger?.Information("Response received but no request callback found for request {RequestId}", response.RequestId);
			}
		}

		private IDisposable StartReceivingAcknowledges()
		{
			_logger?.Debug("Start receiving acknowledges from dispatcher. origin-id={OriginId}", OriginId);

			return _messageDispatcher.OnAcknowledgeReceived().Subscribe(AcknowledgeRequest);
		}

		private void AcknowledgeRequest(IAcknowledgeRequest acknowledgeRequest)
		{
			if (_connectionContexts.TryGetValue(acknowledgeRequest.ConnectionId, out var connectionContext))
			{
				_messageDispatcher.AcknowledgeRequest(connectionContext.LinkId, acknowledgeRequest.AcknowledgeId);
			}
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

		#region IDisposable

		public void Dispose()
		{
			Dispose(true);
		}

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
				_acknowledgeSubscription?.Dispose();
			}
		}

		private void CheckDisposed()
		{
			if (_cts.IsCancellationRequested)
			{
				throw new ObjectDisposedException(GetType().Name);
			}
		}

		#endregion
	}
}
