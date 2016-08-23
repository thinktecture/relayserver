using System;
using System.Collections.Concurrent;
using System.Threading;
using Newtonsoft.Json;
using NLog.Interface;
using Thinktecture.Relay.OnPremiseConnector.SignalR.Messages;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
    public class HeartbeatMonitor : IHeartbeatMonitor
    {
        private readonly ILogger _logger;
        private ConcurrentDictionary<string, ConnectionInformation> ConnectionInformation { get; set; }
        private TimeSpan HeartbeatTimeout { get; set; }
        private string OriginId { get; set; }

        public HeartbeatMonitor(ILogger logger)
        {
            _logger = logger;
        }

        public void Initialize(ConcurrentDictionary<string, ConnectionInformation> connectionInformation, TimeSpan heatbeatTimeout, string originId)
        {
            ConnectionInformation = connectionInformation;
            HeartbeatTimeout = heatbeatTimeout;
            OriginId = originId;
        }

        public void Start()
        {
            if (HeartbeatTimeout.TotalSeconds <= 0)
            {
                _logger.Info("Heartbeat has been disabled");
            }

            var thread = new Thread(Monitor)
            {
                IsBackground = true
            };

            foreach (var connection in ConnectionInformation)
            {
                connection.Value.LastHeartbeatReceived = DateTime.Now;
            }

            thread.Start();
        }

        public void Monitor()
        {
            while (true)
            {
                foreach (var connection in ConnectionInformation)
                {
                    if (connection.Value.Features.Heartbeat && !CheckConnectionTimedOut(connection.Key, connection.Value))
                    {
                        SendHeartbeat(connection.Key, connection.Value);
                    }
                }
                
                Thread.Sleep(HeartbeatTimeout);
            }
        }

        private void SendHeartbeat(string connectionId, ConnectionInformation connectionInformation)
        {
            _logger.Debug("Sending heartbeat to onPremiseId {0} for connection {1}", connectionInformation.LinkId, connectionId);
            connectionInformation.SendHeartbeatAction(CreateHeartbeatRequest());
        }

        private bool CheckConnectionTimedOut(string connectionId, ConnectionInformation connectionInformation)
        {
            if (connectionInformation.LastHeartbeatReceived.AddSeconds(HeartbeatTimeout.TotalSeconds * 2) < DateTime.Now)
            {
                DoConnectionTimedOut(connectionId, connectionInformation.LinkId);
                return true;
            }

            return false;
        }

        private IOnPremiseConnectorRequest CreateHeartbeatRequest()
        {
            return new OnPremiseConnectorRequest
            {
                HttpMethod = "INTERNAL",
                Body = MessageContainer.Create(new HeartbeatMessage()).ToByteArray(),
                // Empty by design
                Url = "",
                RequestStarted = DateTime.UtcNow,
                OriginId = OriginId,
                RequestId = Guid.NewGuid().ToString()
            };
        }

        public event ConnectionTimedOut ConnectionTimedOut;

        private void DoConnectionTimedOut(string connectionId, string onPremiseId)
        {
            var handler = ConnectionTimedOut;

            if (handler != null)
            {
                handler.Invoke(connectionId, onPremiseId);
            }
        }
    }
}