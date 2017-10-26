using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Diagnostics
{
	public interface ITraceManager
	{
		Guid? GetCurrentTraceConfigurationId(Guid linkId);
		void Trace(IOnPremiseConnectorRequest request, IOnPremiseConnectorResponse response, Guid traceConfigurationId);
		Task<IEnumerable<Trace>> GetTracesAsync(Guid traceConfigurationId);
		Task<TraceFile> GetTraceFileAsync(string headerFileName);
	}
}
