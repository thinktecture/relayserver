using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
		private readonly ConcurrentDictionary<string, ConnectionInformation> _onPremises;
		private readonly ConcurrentDictionary<string, HeartbeatInformation> _heartbeatClients;

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
			_onPremises = new ConcurrentDictionary<string, ConnectionInformation>(StringComparer.OrdinalIgnoreCase);
			_requestCompletedCallbacks = new ConcurrentDictionary<string, IOnPremiseConnectorCallback>(StringComparer.OrdinalIgnoreCase);
			_heartbeatClients = new ConcurrentDictionary<string, HeartbeatInformation>();
			_requestSubscriptions = new Dictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
			_cts = new CancellationTokenSource();
			_cancellationToken = _cts.Token;
			OriginId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));

			_logger?.Verbose("Creating backend communication. origin-id={0}", OriginId);
			_logger?.Information("Backend communication is using {0}", messageDispatcher.GetType().Name);
		}

		public void Prepare()
		{
			_linkRepository.DeleteAllConnectionsForOrigin(OriginId);
			_responseSubscription = StartReceivingResponses(OriginId);

			StartSendHeartbeatsLoop(_cts.Token);
		}

		private void CheckDisposed()
		{
			if (_cts.IsCancellationRequested)
			{
				throw new ObjectDisposedException(GetType().Name);
		}
		}

		public Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId)
		{
			CheckDisposed();
			_logger?.Debug("Waiting for response for request {0}", requestId);

			var onPremiseConnectorCallback = _requestCompletedCallbacks[requestId] = _requestCallbackFactory.Create(requestId);

			return GetOnPremiseTargetResponseAsync(onPremiseConnectorCallback, _cancellationToken);
		}

		private async Task<IOnPremiseConnectorResponse> GetOnPremiseTargetResponseAsync(IOnPremiseConnectorCallback callback, CancellationToken cancellationToken)
		{
			try
			{
				using (var timeoutCts = new CancellationTokenSource(_configuration.OnPremiseConnectorCallbackTimeout))
				using (var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token))
				{
					var token = cts.Token;

					using (token.Register(() => callback.Response.TrySetCanceled(token)))
					{
						var response = await callback.Response.Task.ConfigureAwait(false);
						_logger?.Debug("Received on-premise response for request {0}", callback.RequestId);

						return response;
					}
				}
			}
			catch (OperationCanceledException)
			{
				_logger?.Debug("No response received within specified timeout {0} for request {1}", _configuration.OnPremiseConnectorCallbackTimeout, callback.RequestId);
			}
			catch (Exception ex)
			{
				_logger?.Debug(ex, "Error during waiting for on-premise connector response for request {0}", callback.RequestId);
			}
			finally
			{
				_requestCompletedCallbacks.TryRemove(callback.RequestId, out var removed);
			}

			return null;
		}

		public async Task SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching request {0} for link {1}", request.RequestId, linkId);

			await _messageDispatcher.DispatchRequest(linkId, request).ConfigureAwait(false);
		}

		public void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId)
		{
			CheckDisposed();

			if (_onPremises.TryGetValue(connectionId, out var connectionInfo))
			{
				_logger?.Debug("Acknowledging {0} for connection {1}", acknowledgeId, connectionId);
				_messageDispatcher.AcknowledgeRequest(connectionInfo.LinkId, acknowledgeId);
				_linkRepository.RenewActiveConnectionAsync(connectionId);
			}
		}

		public void RegisterOnPremise(RegistrationInformation registrationInformation)
		{
			CheckDisposed();

			_logger?.Debug("Registering link {0} via connection {1} with user name '{2}', role '{3}' and connector version {4}",
				registrationInformation.LinkId,
				registrationInformation.ConnectionId,
				registrationInformation.UserName,
				registrationInformation.Role,
				registrationInformation.ConnectorVersion);

			_linkRepository.AddOrRenewActiveConnectionAsync(registrationInformation.LinkId, OriginId, registrationInformation.ConnectionId, registrationInformation.ConnectorVersion);

			if (!_onPremises.ContainsKey(registrationInformation.ConnectionId))
				_onPremises[registrationInformation.ConnectionId] = new ConnectionInformation(registrationInformation.LinkId, registrationInformation.UserName, registrationInformation.Role);

			lock (_requestSubscriptions)
			{
				if (!_requestSubscriptions.ContainsKey(registrationInformation.ConnectionId))
				{
					_requestSubscriptions[registrationInformation.ConnectionId] = _messageDispatcher.OnRequestReceived(registrationInformation.LinkId, registrationInformation.ConnectionId, !registrationInformation.SupportsAck())
							.Subscribe(request => registrationInformation.RequestAction(request, _cancellationToken));
				}
			}

			RegisterForHeartbeat(registrationInformation);
		}

		public void UnregisterOnPremise(string connectionId)
		{
			CheckDisposed();

			if (_onPremises.TryRemove(connectionId, out var connectionInfo))
			{
				_logger?.Debug("Unregistered on-premise link {0} via connection {1}, user name {2}, role {3}", connectionInfo.LinkId, connectionId, connectionInfo.UserName, connectionInfo.Role);
			}

			_linkRepository.RemoveActiveConnectionAsync(connectionId);
			UnregisterForHeartbeat(connectionId);

			IDisposable requestSubscription;
			lock (_requestSubscriptions)
			{
				if (_requestSubscriptions.TryGetValue(connectionId, out requestSubscription))
					_requestSubscriptions.Remove(connectionId);
			}

			if (requestSubscription != null)
			{
				_logger?.Debug("Disposing request subscription for link {0} for connection {1}", connectionInfo?.LinkId, connectionId);
				requestSubscription.Dispose();
			}
		}

		public async Task SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			CheckDisposed();

			_logger?.Debug("Dispatching response to origin {0}", originId);

			await _messageDispatcher.DispatchResponse(originId, response).ConfigureAwait(false);
		}

		private IDisposable StartReceivingResponses(Guid originId)
		{
			_logger?.Debug("Start receiving responses from dispatcher");

			return _messageDispatcher.OnResponseReceived(originId).Subscribe(ForwardOnPremiseTargetResponse);
		}

		private void ForwardOnPremiseTargetResponse(IOnPremiseConnectorResponse response)
		{
			if (_requestCompletedCallbacks.TryRemove(response.RequestId, out var onPremiseConnectorCallback))
			{
				_logger?.Debug("Forwarding on-premise target response for request {0}", response.RequestId);
				onPremiseConnectorCallback.Response.SetResult(response);
			}
			else
			{
				_logger?.Debug("Response received but no request callback found for request {0}", response.RequestId);
			}
		}

		private void RegisterForHeartbeat(RegistrationInformation registrationInformation)
		{
			if (registrationInformation.SupportsHeartbeat())
			{
				_logger?.Verbose("Registration supports heartbeat. connection-id={0}, version={1}", registrationInformation.ConnectionId, registrationInformation.ConnectorVersion);

				var heartbeatInfo = new HeartbeatInformation()
				{
					ConnectionId = registrationInformation.ConnectionId,
					LinkId = registrationInformation.LinkId,
					ConnectorVersion = registrationInformation.ConnectorVersion,
					RequestAction = registrationInformation.RequestAction,
				};

				_heartbeatClients.TryAdd(registrationInformation.ConnectionId, heartbeatInfo);
			}
			else
			{
				_logger?.Verbose("Registration has no heartbeat support. connection-id={0}, version={1}", registrationInformation.ConnectionId, registrationInformation.ConnectorVersion);
			}
		}

		private void UnregisterForHeartbeat(string connectionId)
		{
			_logger?.Verbose("Unregistering from heartbeating. connection-id={0}", connectionId);

			_heartbeatClients.TryRemove(connectionId, out var info);
		}


		private void StartSendHeartbeatsLoop(CancellationToken token)
		{
			Task.Run(async () =>
			{
				var delay = TimeSpan.FromSeconds(_configuration.ActiveConnectionTimeoutInSeconds / 2d);

				while (!token.IsCancellationRequested)
				{
					await Task.WhenAll(_heartbeatClients.Values.Select(heartbeatClient => SendHeartbeatAsync(heartbeatClient, token))).ConfigureAwait(false);
					await Task.Delay(delay, token).ConfigureAwait(false);
				}
			}, token).ConfigureAwait(false);
		}

		private async Task SendHeartbeatAsync(HeartbeatInformation client, CancellationToken token)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			try
			{
				_logger?.Verbose("Sending heartbeat. connection-id={0}", client.ConnectionId);

				var requestId = Guid.NewGuid().ToString();
				var request = new OnPremiseConnectorRequest()
				{
					HttpMethod = "HEARTBEAT",
					Url = String.Empty,
					RequestStarted = DateTime.UtcNow,
					OriginId = OriginId,
					RequestId = requestId,
					AcknowledgeId = requestId,
					HttpHeaders = new Dictionary<string, string> { ["X-TTRELAY-HEARTBEATINTERVAL"] = (_configuration.ActiveConnectionTimeoutInSeconds / 2).ToString() },
				};

				// heartbeats do NOT go through the message dispatcher as we want to heartbeat the connections directly
				await client.RequestAction(request, token).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				_logger?.Error(ex, "Error during sending heartbeat to a client. LinkId = {0}, ConnectionId = {1}, ConnectorVersion = {2}", client.LinkId, client.ConnectionId, client.ConnectorVersion);
			}
		}

		#region IDisposable

		~BackendCommunication()
		{
			Dispose(false);
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
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
