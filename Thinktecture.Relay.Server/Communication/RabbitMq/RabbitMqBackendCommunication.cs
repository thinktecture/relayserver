using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using EasyNetQ.Topology;
using Newtonsoft.Json;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.OnPremiseConnector.SignalR;
using Thinktecture.Relay.OnPremiseConnector.SignalR.Messages;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.Dto;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
    internal class RabbitMqBackendCommunication : BackendCommunication
    {
        private static readonly int _expiration = (int) TimeSpan.FromSeconds(10).TotalMilliseconds;

        private readonly IConfiguration _configuration;
        private readonly IOnPremiseConnectorCallbackFactory _onPremiseConnectorCallbackFactory;
        private readonly ILogger _logger;
        private readonly ConcurrentDictionary<string, IOnPremiseConnectorCallback> _callbacks;
        private readonly ConcurrentDictionary<string, IDisposable> _onPremiseConsumers;
        private readonly ConcurrentDictionary<string, ConnectionInformation> _onPremises;
        private readonly ConcurrentDictionary<string, IQueue> _declaredQueues;

        private IBus _bus;
        private IDisposable _consumer;

        public RabbitMqBackendCommunication(IConfiguration configuration, IRabbitMqBusFactory busFactory, IOnPremiseConnectorCallbackFactory onPremiseConnectorCallbackFactory, ILogger logger, IHeartbeatMonitor heartbeatMonitor) 
            : base(logger)
        {
            _configuration = configuration;
            _onPremiseConnectorCallbackFactory = onPremiseConnectorCallbackFactory;
            _logger = logger;
            _bus = busFactory.CreateBus();

            _callbacks = new ConcurrentDictionary<string, IOnPremiseConnectorCallback>();
            _onPremiseConsumers = new ConcurrentDictionary<string, IDisposable>();
            _onPremises = new ConcurrentDictionary<string, ConnectionInformation>();
            _declaredQueues = new ConcurrentDictionary<string, IQueue>();

            StartReceivingOnPremiseTargetResponses(OriginId);

            heartbeatMonitor.Initialize(_onPremises, configuration.HeartbeatTimeout, OriginId);
            heartbeatMonitor.ConnectionTimedOut += HeartbeatMonitorConnectionTimedOut;
            heartbeatMonitor.Start();
        }

        private void HeartbeatMonitorConnectionTimedOut(string connectionId, string onPremiseId)
        {
            _logger.Warn("OnPremise {0} has a connection timeout for {1}", onPremiseId, connectionId);
            UnregisterOnPremise(connectionId);
        }

        public override Task<IOnPremiseTargetReponse> GetResponseAsync(string requestId)
        {
            CheckDisposed();

            _logger.Debug("Waiting for response for request '{0}'", requestId);

            var onPremiseConnectorCallback = _onPremiseConnectorCallbackFactory.Create(requestId);
            _callbacks[requestId] = onPremiseConnectorCallback;

            var task = Task<IOnPremiseTargetReponse>.Factory.StartNew(WaitForOnPremiseTargetResponse, onPremiseConnectorCallback);
            return task;
        }

        private IOnPremiseTargetReponse WaitForOnPremiseTargetResponse(object state)
        {
            var onPremiseConnectorCallback = (IOnPremiseConnectorCallback) state;
            if (onPremiseConnectorCallback.Handle.WaitOne(_configuration.OnPremiseConnectorCallbackTimeout))
            {
                _logger.Debug("On-Premise Target response for request '{0}'", onPremiseConnectorCallback.RequestId);
                return onPremiseConnectorCallback.Reponse;
            }

            _callbacks.TryRemove(onPremiseConnectorCallback.RequestId, out onPremiseConnectorCallback);
            return null;
        }

        public override async Task SendOnPremiseConnectorRequest(string onPremiseId, IOnPremiseConnectorRequest onPremiseConnectorRequest)
        {
            CheckDisposed();

            _logger.Debug("Sending client request for On-Premise Connector '{0}'", onPremiseId);

            var queue = DeclareOnPremiseQueue(onPremiseId);
            await _bus.Advanced.PublishAsync(Exchange.GetDefault(), queue.Name, false, false, new Message<string>(JsonConvert.SerializeObject(onPremiseConnectorRequest)));
        }

        public override void RegisterOnPremise(RegistrationInformation registrationInformation)
        {
            CheckDisposed();

            UnregisterOnPremise(registrationInformation.ConnectionId);

            _logger.Debug("Registering OnPremise '{0}' via connection '{1}'", registrationInformation.OnPremiseId, registrationInformation.ConnectionId);

            var queue = DeclareOnPremiseQueue(registrationInformation.OnPremiseId);

            var consumer = _bus.Advanced.Consume(queue, (Action<IMessage<string>, MessageReceivedInfo>) ((message, info) => registrationInformation.RequestAction(JsonConvert.DeserializeObject<OnPremiseConnectorRequest>(message.Body))));
            _onPremiseConsumers[registrationInformation.ConnectionId] = consumer;
            _onPremises[registrationInformation.ConnectionId] = new ConnectionInformation(registrationInformation.OnPremiseId, registrationInformation.SendHeartbeatAction);
        }

        private IQueue DeclareOnPremiseQueue(string onPremiseId)
        {
            var queueName = "OnPremises " + onPremiseId;
            return _declaredQueues.GetOrAdd(queueName, DeclareQueue);
        }

        public override void UnregisterOnPremise(string connectionId)
        {
            CheckDisposed();

            ConnectionInformation onPremiseInformation;
            if (_onPremises.TryRemove(connectionId, out onPremiseInformation))
            {
                IQueue queue;
                _declaredQueues.TryRemove("OnPremises " + onPremiseInformation.LinkId, out queue);
            }

            string onPremiseId = onPremiseInformation == null ? "unknown" : onPremiseInformation.LinkId;
            
            IDisposable consumer;
            if (_onPremiseConsumers.TryRemove(connectionId, out consumer))
            {
                _logger.Debug("Unregistering OnPremise '{0} via connection '{0}'", onPremiseId, connectionId);
                consumer.Dispose();
            }
            else
            {
                _logger.Debug("Unregistering OnPremise '{0} via connection '{0}' without consumer", onPremiseId, connectionId);
            }
        }

        public override async Task SendOnPremiseTargetResponse(string originId, IOnPremiseTargetReponse reponse)
        {
            CheckDisposed();

            _logger.Debug("Sending On-Premise Target response to origin '{0}'", originId);

            var queue = DeclareRelayServerQueue(originId);
            await _bus.Advanced.PublishAsync(Exchange.GetDefault(), queue.Name, false, false, new Message<string>(JsonConvert.SerializeObject(reponse)));
        }

        public override bool IsRegistered(string connectionId)
        {
            return _onPremises.Any(o => o.Value.LinkId.Equals(connectionId, StringComparison.OrdinalIgnoreCase));
        }

        public override List<string> GetConnections(string linkId)
        {
            return _onPremises.Where(p=>p.Value.LinkId.Equals(linkId, StringComparison.OrdinalIgnoreCase)).Select(p=>p.Key).ToList();
        }

        public override void HeartbeatReceived(string connectionId)
        {
            var connection = _onPremises[connectionId];

            if (connection == null)
            {
                _logger.Warn("Received heartbeat for connection {0}, but it was not found.", connectionId);
                return;
            }

            connection.LastHeartbeatReceived = DateTime.Now;
        }

        public override void EnableConnectionFeatures(Features features, string connectionId)
        {
            var connection = _onPremises[connectionId];

            if (connection == null)
            {
                _logger.Warn("Trying to set features for connection {0}, but it was not found.", connectionId);
                return;
            }

            connection.Features.Heartbeat = features.Heartbeat;
            _logger.Info("Set features for connection {0}: Heartbeat {1}", connection, features.Heartbeat);
        }

        private void StartReceivingOnPremiseTargetResponses(string originId)
        {
            _logger.Debug("Start receiving On-Premise Target responses for origin '{0}'", originId);

            var queue = DeclareRelayServerQueue(originId);
            _bus.Advanced.Consume(queue, (Action<IMessage<string>, MessageReceivedInfo>) ((message, info) => ForwardOnPremiseTargetResponse(JsonConvert.DeserializeObject<OnPremiseTargetReponse>(message.Body))));
        }

        private IQueue DeclareRelayServerQueue(string originId)
        {
            var queueName = "RelayServer " + originId;
            return _declaredQueues.GetOrAdd(queueName, DeclareQueue);
        }

        private void ForwardOnPremiseTargetResponse(IOnPremiseTargetReponse reponse)
        {
            _logger.Debug("Forwarding On-Premise Target response for request '{0}'", reponse.RequestId);

            IOnPremiseConnectorCallback onPremiseConnectorCallback;
            if (_callbacks.TryRemove(reponse.RequestId, out onPremiseConnectorCallback))
            {
                onPremiseConnectorCallback.Reponse = reponse;
                onPremiseConnectorCallback.Handle.Set();
            }
            else
            {
                _logger.Debug("No callback found for request '{0}'", reponse.RequestId);
            }
        }

        private IQueue DeclareQueue(string queueName)
        {
            _logger.Debug("Creating queue '{0}'", queueName);

            var queue = _bus.Advanced.QueueDeclare(queueName, expires: _expiration);
            return queue;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                if (_consumer != null)
                {
                    _consumer.Dispose();
                    _consumer = null;
                }

                foreach (var consumer in _onPremiseConsumers.Values)
                {
                    consumer.Dispose();
                }

                if (_bus != null)
                {
                    _bus.Dispose();
                    _bus = null;
                }
            }
        }
    }
}
