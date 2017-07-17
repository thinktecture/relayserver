using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.Configuration;

namespace Thinktecture.Relay.Server.Communication
{
    internal class BackendCommunication : IBackendCommunication, IDisposable
    {
        private bool _disposed;

        private readonly IConfiguration _configuration;
        private readonly IMessageDispatcher _messageDispatcher;
        private readonly IOnPremiseConnectorCallbackFactory _requesetCallbackFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IOnPremiseConnectorCallback> _requestCompletedCallbacks;
        private readonly ConcurrentDictionary<string, ConnectionInformation> _onPremises;

        private readonly ConcurrentDictionary<string, IDisposable> _requestSubscriptions;
        private readonly IDisposable _responseSubscription;

        public string OriginId { get; }

        public BackendCommunication(IConfiguration configuration, IMessageDispatcher messageDispatcher, IOnPremiseConnectorCallbackFactory requesetCallbackFactory, ILogger logger)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));
            if (messageDispatcher == null)
                throw new ArgumentNullException(nameof(messageDispatcher));
            if (requesetCallbackFactory == null)
                throw new ArgumentNullException(nameof(requesetCallbackFactory));
            if (logger == null)
                throw new ArgumentNullException(nameof(logger));

            OriginId = Guid.NewGuid().ToString();
            logger.Trace("Creating backend communication with origin id {0}", OriginId);
            logger.Info("Backend communication is using {0}", messageDispatcher.GetType().Name);

            _configuration = configuration;
            _messageDispatcher = messageDispatcher;
            _requesetCallbackFactory = requesetCallbackFactory;
            _logger = logger;

            _onPremises = new ConcurrentDictionary<string, ConnectionInformation>(StringComparer.OrdinalIgnoreCase);
            _requestCompletedCallbacks = new ConcurrentDictionary<string, IOnPremiseConnectorCallback>(StringComparer.OrdinalIgnoreCase);
            _requestSubscriptions = new ConcurrentDictionary<string, IDisposable>(StringComparer.OrdinalIgnoreCase);
            _responseSubscription = StartReceivingResponses(OriginId);
        }

        protected void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        public Task<IOnPremiseTargetResponse> GetResponseAsync(string requestId)
        {
            CheckDisposed();
            _logger.Debug("Waiting for response for request id {0}", requestId);

            var onPremiseConnectorCallback = _requestCompletedCallbacks[requestId] = _requesetCallbackFactory.Create(requestId);

            return Task<IOnPremiseTargetResponse>.Factory.StartNew(() => WaitForOnPremiseTargetResponse(onPremiseConnectorCallback));
        }

        private IOnPremiseTargetResponse WaitForOnPremiseTargetResponse(IOnPremiseConnectorCallback onPremiseConnectorCallback)
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

        public async Task SendOnPremiseConnectorRequest(string onPremiseId, IOnPremiseTargetRequest onPremiseTargetRequest)
        {
            CheckDisposed();

            _logger.Debug("Sending request for on-premise connector {0} to message dispatcher", onPremiseId);

            await _messageDispatcher.DispatchRequest(onPremiseId, onPremiseTargetRequest);
        }

        public void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId)
        {
            CheckDisposed();

            ConnectionInformation onPremiseInformation;
            if (_onPremises.TryGetValue(connectionId, out onPremiseInformation))
            {
                _logger.Debug("Acknowledging {0} for on-premise connection id {1}. OnPremise Id: {2}", acknowledgeId, connectionId, onPremiseInformation.OnPremiseId);
                _messageDispatcher.AcknowledgeRequest(onPremiseInformation.OnPremiseId, acknowledgeId);
            }
        }

        public void RegisterOnPremise(RegistrationInformation registrationInformation)
        {
            CheckDisposed();

            _logger.Debug("Registering on-premise {0} via connection {1}. User name: {2}, Role: {3}, Connector Version: {4}", registrationInformation.OnPremiseId, registrationInformation.ConnectionId, registrationInformation.UserName, registrationInformation.Role, registrationInformation.ConnectorVersion);

            if (!_onPremises.ContainsKey(registrationInformation.ConnectionId))
                _onPremises[registrationInformation.ConnectionId] = new ConnectionInformation(registrationInformation.OnPremiseId, registrationInformation.UserName, registrationInformation.Role);

            if (!_requestSubscriptions.ContainsKey(registrationInformation.ConnectionId))
            {
                _requestSubscriptions[registrationInformation.ConnectionId] = _messageDispatcher.OnRequestReceived(registrationInformation.OnPremiseId, registrationInformation.ConnectionId, registrationInformation.ConnectorVersion == 0)
                    .Subscribe(request => registrationInformation.RequestAction(request));
            }
        }

        public void UnregisterOnPremise(string connectionId)
        {
            CheckDisposed();

            ConnectionInformation onPremiseInformation;
            if (_onPremises.TryRemove(connectionId, out onPremiseInformation))
                _logger.Debug("Unregistered on-premise connection id {0}. OnPremise Id: {1}, User name: {2}, Role: {3}", connectionId, onPremiseInformation.OnPremiseId, onPremiseInformation.UserName, onPremiseInformation.Role);

            IDisposable requestSubscription;
            if (_requestSubscriptions.TryRemove(connectionId, out requestSubscription))
            {
                _logger.Debug("Disposing request subscription for OnPremise id {0} and connection id {1}", onPremiseInformation?.OnPremiseId ?? "unknown", connectionId);
                requestSubscription.Dispose();
            }
        }

        public async Task SendOnPremiseTargetResponse(string originId, IOnPremiseTargetResponse response)
        {
            CheckDisposed();

            _logger.Debug("Sending response to origin id {0}", originId);

            await _messageDispatcher.DispatchResponse(originId, response);
        }

        public bool IsRegistered(string connectionId)
        {
            return _onPremises.Any(o => o.Value.OnPremiseId.Equals(connectionId, StringComparison.OrdinalIgnoreCase));
        }

        public List<string> GetConnections(string linkId)
        {
            return _onPremises.Where(p => p.Value.OnPremiseId.Equals(linkId, StringComparison.OrdinalIgnoreCase)).Select(p => p.Key).ToList();
        }

        private IDisposable StartReceivingResponses(string originId)
        {
            _logger.Debug("Start receiving responses for origin id {0}", originId);

            return _messageDispatcher.OnResponseReceived(originId)
                .Subscribe(ForwardOnPremiseTargetResponse);
        }

        private void ForwardOnPremiseTargetResponse(IOnPremiseTargetResponse response)
        {
            IOnPremiseConnectorCallback onPremiseConnectorCallback;
            if (_requestCompletedCallbacks.TryRemove(response.RequestId, out onPremiseConnectorCallback))
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

        #region IDisposable

        ~BackendCommunication()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;

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