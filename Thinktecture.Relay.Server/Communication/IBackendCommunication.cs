using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId);
		void SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request);
		void AcknowledgeOnPremiseConnectorRequest(Guid originId, string acknowledgeId, string connectionId);
		Task RegisterOnPremiseAsync(RegistrationInformation registrationInformation);
		Task UnregisterOnPremiseAsync(string connectionId);
		void SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response);
	}
}
