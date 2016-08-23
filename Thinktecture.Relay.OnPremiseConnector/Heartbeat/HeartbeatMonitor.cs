using System;
using System.Configuration;
using System.Threading;
using NLog.Interface;

namespace Thinktecture.Relay.OnPremiseConnector.Heartbeat
{
    public class HeartbeatMonitor : IHeartbeatMonitor
    {
        private readonly ILogger _logger;
        private CancellationTokenSource _cancellationTokenSource;
        private static readonly object _lock = new object();
        private DateTime _lastHeartbeatTime;

        public TimeSpan Timeout { get; set; }

        public event ConnectionTimedOut ConnectionTimedOut;

        public DateTime LastHeartbeatReceived
        {
            get
            {
                lock (_lock)
                {
                    return _lastHeartbeatTime;
                }
            }
            set
            {
                lock (_lock)
                {
                    _lastHeartbeatTime = value;
                }
            }
        }

        public HeartbeatMonitor(ILogger logger)
        {
            _logger = logger;
        }

        public void Start()
        {
            if (Timeout.TotalSeconds <= 0)
            {
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();

            var thread = new Thread(() => Monitor(Timeout, _cancellationTokenSource.Token))
            {
                IsBackground = true
            };

            _logger.Info("Starting heartbeats");
            LastHeartbeatReceived = DateTime.Now;
            thread.Start();
        }

        public void Stop()
        {
            if (_cancellationTokenSource != null)
            {
                _logger.Info("Stopping heartbeats");
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource = null;
            }
        }

        public event SendHeartbeatRequest SendHeartbeatRequest;

        private void Monitor(TimeSpan timeout, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                CheckHeartbeat();
                DoSendHeartbeatRequest();
                Thread.Sleep(timeout);
            }
        }

        private void CheckHeartbeat()
        {
            if (LastHeartbeatReceived.AddSeconds(Timeout.TotalSeconds * 2) < DateTime.Now)
            {
                DoConnectionTimedOut();
            }
        }

        private void DoSendHeartbeatRequest()
        {
            var handler = SendHeartbeatRequest;

            if (handler != null)
            {
                handler.Invoke();
            }
        }

        private void DoConnectionTimedOut()
        {
            var handler = ConnectionTimedOut;

            if (handler != null)
            {
                handler.Invoke();
            }
        }
    }
}