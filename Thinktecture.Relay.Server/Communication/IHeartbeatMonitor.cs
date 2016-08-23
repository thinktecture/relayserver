using System;
using System.Collections.Concurrent;

namespace Thinktecture.Relay.Server.Communication
{
    public delegate void ConnectionTimedOut(string connectionId, string onPremiseId);

    public interface IHeartbeatMonitor
    {
        void Start();
        void Initialize(ConcurrentDictionary<string, ConnectionInformation> connectionInformation, TimeSpan heatbeatTimeout, string originId);
        event ConnectionTimedOut ConnectionTimedOut;
    }
} 