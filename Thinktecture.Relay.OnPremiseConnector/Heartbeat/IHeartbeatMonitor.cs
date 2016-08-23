using System;

namespace Thinktecture.Relay.OnPremiseConnector.Heartbeat
{
    public delegate void SendHeartbeatRequest();

    public delegate void ConnectionTimedOut();
   
    public interface IHeartbeatMonitor
    {
        TimeSpan Timeout { get; set; }
        void Start();
        void Stop();
        event SendHeartbeatRequest SendHeartbeatRequest;
        event ConnectionTimedOut ConnectionTimedOut;
        DateTime LastHeartbeatReceived { get; set; }
    }
}