using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId);
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId, TimeSpan requestTimeout);
		Task SendOnPremiseConnectorRequestAsync(Guid linkId, IOnPremiseConnectorRequest request);
		Task AcknowledgeOnPremiseConnectorRequestAsync(string connectionId, string acknowledgeId);
		Task RenewLastActivityAsync(string connectionId);
		Task RegisterOnPremiseAsync(IOnPremiseConnectionContext onPremiseConnectionContext);
		Task UnregisterOnPremiseAsync(string connectionId);
		Task DeactivateOnPremiseAsync(string connectionId);
		Task SendOnPremiseTargetResponseAsync(Guid originId, IOnPremiseConnectorResponse response);
		IEnumerable<IOnPremiseConnectionContext> GetConnectionContexts();
	}
}
