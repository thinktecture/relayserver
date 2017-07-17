using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface ITraceManager
	{
		Guid? GetCurrentTraceConfigurationId(Guid linkId);
		void Trace(IOnPremiseConnectorRequest onPremiseConnectorRequest, IOnPremiseTargetResponse onPremiseTargetResponse, Guid traceConfigurationId);
		Task<IEnumerable<Trace>> GetTracesAsync(Guid traceConfigurationId);
	    Task<TraceFile> GetTraceFileAsync(string headerFileName);
	}
}