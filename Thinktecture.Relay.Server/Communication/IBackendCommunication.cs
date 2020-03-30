using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId, TimeSpan? requestTimeout = null);
		void SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request);
		Task AcknowledgeOnPremiseConnectorRequestAsync(Guid originId, string connectionId, string acknowledgeId);
		Task RenewLastActivityAsync(string connectionId);
		Task RegisterOnPremiseAsync(IOnPremiseConnectionContext onPremiseConnectionContext);
		Task UnregisterOnPremiseConnectionAsync(string connectionId);
		Task DeactivateOnPremiseConnectionAsync(string connectionId);
		void SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response);
		IEnumerable<IOnPremiseConnectionContext> GetConnectionContexts();
	}
}
