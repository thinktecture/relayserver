using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.Server.Configuration;
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
		private readonly ConcurrentDictionary<string, HeartbeatInformation> _heartbeatableClients;

		private readonly ConcurrentDictionary<string, IDisposable> _requestSubscriptions;
		private readonly IDisposable _responseSubscription;

		public Guid OriginId { get; }

		public BackendCommunication(IConfiguration configuration, IMessageDispatcher messageDispatcher, IOnPremiseConnectorCallbackFactory requesetCallbackFactory, ILogger logger, IPersistedSettings persistedSettings, ILinkRepository linkRepository)
		{
			_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
			_messageDispatcher = messageDispatcher ?? throw new ArgumentNullException(nameof(messageDispatcher));
			_requestCallbackFactory = requesetCallbackFactory ?? throw new ArgumentNullException(nameof(requesetCallbackFactory));
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_linkRepository = linkRepository ?? throw new ArgumentNullException(nameof(linkRepository));

			OriginId = persistedSettings?.OriginId ?? throw new ArgumentNullException(nameof(persistedSettings));
			_cts = new CancellationTokenSource();
			_cancellationToken = _cts.Token;
			_linkRepository.DeleteAllConnectionsForOrigin(OriginId);

			logger.Trace("Creating backend communication with origin id {0}", OriginId);
			logger.Info("Backend communication is using {0}", messageDispatcher.GetType().Name);

			_onPremises = new ConcurrentDictionary<string, ConnectionInformation>(StringComparer.OrdinalIgnoreCase);
			_requestCompletedCallbacks = new ConcurrentDictionary<string, IOnPremiseConnectorCallback>(StringComparer.OrdinalIgnoreCase);
			_requestSubscriptions = new ConcurrentDictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
			_heartbeatableClients = new ConcurrentDictionary<string, HeartbeatInformation>();

			_responseSubscription = StartReceivingResponses(OriginId);

#pragma warning disable 4014
			// dont await, heartbeat in the background
			HeartbeatAllClientsAsync(_cts.Token);
#pragma warning restore 4014
		}

		protected void CheckDisposed()
		{
			if (_cts.IsCancellationRequested)
				throw new ObjectDisposedException(GetType().Name);
		}

		public Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId)
		{
			CheckDisposed();
			_logger.Debug("Waiting for response for request id {0}", requestId);

			var onPremiseConnectorCallback = _requestCompletedCallbacks[requestId] = _requestCallbackFactory.Create(requestId);

			return Task.Run(() => WaitForOnPremiseTargetResponse(onPremiseConnectorCallback), _cancellationToken);
		}

		private IOnPremiseConnectorResponse WaitForOnPremiseTargetResponse(IOnPremiseConnectorCallback onPremiseConnectorCallback)
		{
			try
			{
				if (onPremiseConnectorCallback.Handle.WaitOne(_configuration.OnPremiseConnectorCallbackTimeout))
				{
					_logger.Debug("Received On-Premise response for request id {0}", onPremiseConnectorCallback.RequestId);

					return onPremiseConnectorCallback.Response;
				}

				_logger.Debug("No response received within specified timeout {0}. Request id {1}", _configuration.OnPremiseConnectorCallbackTimeout, onPremiseConnectorCallback.RequestId);
				return null;
			}
			finally
			{
				_requestCompletedCallbacks.TryRemove(onPremiseConnectorCallback.RequestId, out onPremiseConnectorCallback);
			}
		}

		public async Task SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request)
		{
			CheckDisposed();

			_logger.Debug("Sending request for on-premise connector link {0} to message dispatcher", linkId);

			await _messageDispatcher.DispatchRequest(linkId, request);
		}

		public void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId)
		{
			CheckDisposed();

			if (_onPremises.TryGetValue(connectionId, out var connectionInfo))
			{
				_logger.Debug("Acknowledging {0} for on-premise connection id {1}. OnPremise Link Id: {2}", acknowledgeId, connectionId, connectionInfo.LinkId);
				_messageDispatcher.AcknowledgeRequest(connectionInfo.LinkId, acknowledgeId);
				_linkRepository.RenewActiveConnectionAsync(connectionId);
			}
		}

		public void RegisterOnPremise(RegistrationInformation registrationInformation)
		{
			CheckDisposed();

			_logger.Debug("Registering on-premise link {0} via connection {1}. User name: {2}, Role: {3}, Connector Version: {4}", registrationInformation.LinkId, registrationInformation.ConnectionId, registrationInformation.UserName, registrationInformation.Role, registrationInformation.ConnectorVersion);

			_linkRepository.AddOrRenewActiveConnectionAsync(registrationInformation.LinkId, OriginId, registrationInformation.ConnectionId, registrationInformation.ConnectorVersion);

			if (!_onPremises.ContainsKey(registrationInformation.ConnectionId))
				_onPremises[registrationInformation.ConnectionId] = new ConnectionInformation(registrationInformation.LinkId, registrationInformation.UserName, registrationInformation.Role);

			RegisterForHeartbeat(registrationInformation, _cancellationToken);

			if (!_requestSubscriptions.ContainsKey(registrationInformation.ConnectionId))
			{
				_requestSubscriptions[registrationInformation.ConnectionId] = _messageDispatcher.OnRequestReceived(registrationInformation.LinkId, registrationInformation.ConnectionId, !registrationInformation.SupportsAck())
					.Subscribe(request => registrationInformation.RequestAction(request, _cancellationToken));
			}
		}

		public void UnregisterOnPremise(string connectionId)
		{
			CheckDisposed();

			if (_onPremises.TryRemove(connectionId, out var connectionInfo))
			{
				_logger.Debug("Unregistered on-premise connection id {0}. OnPremise Id: {1}, User name: {2}, Role: {3}", connectionId, connectionInfo.LinkId, connectionInfo.UserName, connectionInfo.Role);
			}

			_linkRepository.RemoveActiveConnectionAsync(connectionId);
			UnregisterForHeartbeat(connectionId);

			if (_requestSubscriptions.TryRemove(connectionId, out var requestSubscription))
			{
				_logger.Debug("Disposing request subscription for OnPremise id {0} and connection id {1}", connectionInfo?.LinkId, connectionId);
				requestSubscription.Dispose();
			}
		}

		public async Task SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response)
		{
			CheckDisposed();

			_logger.Debug("Sending response to origin id {0}", originId);

			await _messageDispatcher.DispatchResponse(originId, response);
		}

		private IDisposable StartReceivingResponses(Guid originId)
		{
			_logger.Debug("Start receiving responses for origin id {0}", originId);

			return _messageDispatcher.OnResponseReceived(originId)
				.Subscribe(ForwardOnPremiseTargetResponse);
		}

		private void ForwardOnPremiseTargetResponse(IOnPremiseConnectorResponse response)
		{
			if (_requestCompletedCallbacks.TryRemove(response.RequestId, out var onPremiseConnectorCallback))
			{
				_logger.Debug("Forwarding on-premise target response for request id {0}", response.RequestId);

				onPremiseConnectorCallback.Response = response;
				onPremiseConnectorCallback.Handle.Set();
			}
			else
			{
				_logger.Debug("Response received but no request callback found for request id {0}", response.RequestId);
			}
		}

		private void RegisterForHeartbeat(RegistrationInformation registrationInformation, CancellationToken cancellationToken)
		{
			if (registrationInformation.SupportsHeartbeat())
			{
				_logger.Trace($"{nameof(BackendCommunication)}: Connection with id {registrationInformation.ConnectionId} has connector version {registrationInformation.ConnectorVersion} and will be registered for heartbeating.");

				var heartbeatInfo = new HeartbeatInformation()
				{
					ConnectionId = registrationInformation.ConnectionId,
					LinkId = registrationInformation.LinkId,
					ConnectorVersion = registrationInformation.ConnectorVersion,
					RequestAction = registrationInformation.RequestAction,
				};

				InitializeHeartbeatAsync(heartbeatInfo, cancellationToken);
				_heartbeatableClients.TryAdd(registrationInformation.ConnectionId, heartbeatInfo);
			}
			else
			{
				_logger.Trace($"Connection with id {registrationInformation.ConnectionId} has connector version {registrationInformation.ConnectorVersion} and is not capable of heartbeating.");
			}
		}

		private void UnregisterForHeartbeat(string connectionId)
		{
			_logger.Trace($"Unregistering connection with id {connectionId} from heartbeating.");

			_heartbeatableClients.TryRemove(connectionId, out var heartbeatInformation);
		}

		private async void InitializeHeartbeatAsync(HeartbeatInformation heartbeatInfo, CancellationToken token)
		{
			_logger.Trace($"Initializing heartbeat with connection {heartbeatInfo.ConnectionId}");

			var requestId = Guid.NewGuid().ToString();
			var request = new OnPremiseConnectorRequest()
			{
				HttpMethod = "INIT_HEARTBEAT",
				Url = String.Empty,
				RequestStarted = DateTime.UtcNow,
				OriginId = OriginId,
				RequestId = requestId,
				AcknowledgeId = requestId,
				HttpHeaders = new Dictionary<string, string>()
				{
					{ "X-TTRELAY-HEARTBEATINTERVAL", (_configuration.ActiveConnectionTimeoutInSeconds / 2).ToString() },
				},
			};

			var responseTask = GetResponseAsync(requestId);
			await heartbeatInfo.RequestAction(request, token);

			var response = await responseTask;

			if (response == null)
			{
				_logger.Warn($"Connection {heartbeatInfo.ConnectionId} did NOT respond to heartbeat in time.");
			}
		}

		private async Task HeartbeatAllClientsAsync(CancellationToken token)
		{
			await Task.Run(async () =>
			{
				var delayBetweenHeartbeats = _configuration.ActiveConnectionTimeoutInSeconds / 2 * 1000;
				Task[] heartbeatTasks = null;

				while (!token.IsCancellationRequested)
				{
					var clients = _heartbeatableClients.Values;

					if (clients.Count > 0)
					{
						if (heartbeatTasks?.Length != clients.Count)
							heartbeatTasks = new Task[clients.Count];

						var index = 0;

						foreach (var client in clients)
						{
							heartbeatTasks[index] = SendHeartbeatAsync(client, token);
							index++;
						}

						await Task.WhenAll(heartbeatTasks);
					}

					await Task.Delay(delayBetweenHeartbeats, token);
				}
			}, token);
		}

		private async Task SendHeartbeatAsync(HeartbeatInformation client, CancellationToken token)
		{
			if (client == null)
				throw new ArgumentNullException(nameof(client));

			try
			{
				_logger.Trace($"{nameof(BackendCommunication)}: Sending heartbeat to connection {client.ConnectionId}");

				var requestId = Guid.NewGuid().ToString();
				var request = new OnPremiseConnectorRequest()
				{
					HttpMethod = "HEARTBEAT",
					Url = String.Empty,
					RequestStarted = DateTime.UtcNow,
					OriginId = OriginId,
					RequestId = requestId,
					AcknowledgeId = requestId,
				};

				// heartbeat do NOT go through the message dispatcher as we want to heartbeat the connections directly
				await client.RequestAction(request, token);
			}
			catch (Exception ex)
			{
				_logger.Error(ex, $"{nameof(BackendCommunication)}: Error during sending heartbeat to a client. LinkId = {{0}}, ConnectionId = {{1}}, ConnectorVersion = {{2}}", client.LinkId, client.ConnectionId, client.ConnectorVersion);
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

				foreach (var consumer in _requestSubscriptions.Values)
				{
					consumer.Dispose();
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
