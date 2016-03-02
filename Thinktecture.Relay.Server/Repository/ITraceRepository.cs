using System;
using System.Collections.Generic;
using Thinktecture.Relay.Server.Dto;

namespace Thinktecture.Relay.Server.Repository
{
	public interface ITraceRepository
	{
		void Create(TraceConfiguration traceConfiguration);
		bool Disable(Guid traceConfigurationId);
		Guid? GetCurrentTraceConfigurationId(Guid linkId);
		IEnumerable<TraceConfiguration> GetTraceConfigurations(Guid linkId);
	    TraceConfiguration GetRunningTranceConfiguration(Guid linkId);
	    TraceConfiguration GetTraceConfiguration(Guid traceConfigurationId);
	}
}