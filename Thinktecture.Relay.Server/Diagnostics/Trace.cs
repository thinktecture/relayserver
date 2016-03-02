using System;

namespace Thinktecture.Relay.Server.Diagnostics
{
    public class Trace
    {
        public TraceFile OnPremiseConnectorTrace { get; set; }
        public TraceFile OnPremiseTargetTrace { get; set; }
        public DateTime TracingDate { get; set; }
    }
}