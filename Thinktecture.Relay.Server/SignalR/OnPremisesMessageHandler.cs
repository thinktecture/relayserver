using System;
using System.Text;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.SignalR;
using Thinktecture.Relay.OnPremiseConnector.SignalR.Messages;
using Thinktecture.Relay.Server.Communication;
using Thinktecture.Relay.Server.Configuration;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.SignalR
{
    public class OnPremisesMessageHandler : IOnPremisesMessageHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IBackendCommunication _backendCommunication;
        private readonly ILogger _logger;

        public OnPremisesMessageHandler(IConfiguration configuration, IBackendCommunication backendCommunication, ILogger logger)
        {
            _configuration = configuration;
            _backendCommunication = backendCommunication;
            _logger = logger;
        }

        public void Received(IConnection connection, IRequest request, string connectionId, string data)
        {
            _logger.Trace("Received OnPremiseConnection message from {0} with data {1}", connectionId, data);

            var messageContainer = JsonConvert.DeserializeObject<MessageContainer>(data);

            switch (messageContainer.Type)
            {
                case MessageType.Feature:
                    _logger.Trace("Received features message", messageContainer.Message);
                    HandleFeaturesMessage(connection, connectionId, JsonConvert.DeserializeObject<FeaturesMessage>(messageContainer.Message));
                    break;

                case MessageType.Heartbeat:
                    _logger.Trace("Received heartbeat from {0}", connectionId);
                    HandleHeartbeatMessage(connectionId);
                    break;

                default:
                    _logger.Trace("Received unknown message", data);
                    break;
            }
        }

        private void HandleHeartbeatMessage(string connectionId)
        {
            _backendCommunication.HeartbeatReceived(connectionId);
        }

        private void HandleFeaturesMessage(IConnection connection, string connectionId, FeaturesMessage message)
        {
            if (message.Features.Heartbeat)
            {
                SendHeartbeatConfiguration(connection, connectionId);
            }

            _backendCommunication.EnableConnectionFeatures(message.Features, connectionId);
        }

        private void SendHeartbeatConfiguration(IConnection connection, string connectionId)
        {
            var message = CreateMessage(new HeartbeatConfigurationMessage()
            {
                Timeout = _configuration.HeartbeatTimeout
            });

            connection.Send(connectionId, message);
        }

        private OnPremiseTargetRequest CreateMessage(IOnPremiseMessage message)
        {
            return new OnPremiseTargetRequest()
            {
                Body = MessageContainer.Create(message).ToBase64String(),
                HttpMethod = "INTERNAL",
                OriginId = _backendCommunication.OriginId
            };
        }
    }
}