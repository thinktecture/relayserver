using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId);
		Task SendOnPremiseConnectorRequestAsync(Guid linkId, IOnPremiseConnectorRequest request);
		void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId);
		Task RegisterOnPremiseAsync(RegistrationInformation registrationInformation);
		Task UnregisterOnPremiseAsync(string connectionId);
		Task SendOnPremiseTargetResponseAsync(Guid originId, IOnPremiseConnectorResponse response);
	}
}
