using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseConnectorResponse> GetResponseAsync(string requestId);
		Task SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseConnectorRequest request);
		void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId);
		void RegisterOnPremise(RegistrationInformation registrationInformation);
		void UnregisterOnPremise(string connectionId);
		Task SendOnPremiseTargetResponse(Guid originId, IOnPremiseConnectorResponse response);
	}
}
