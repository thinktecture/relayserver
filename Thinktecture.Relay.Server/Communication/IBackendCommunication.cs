using System;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		Guid OriginId { get; }
		Task<IOnPremiseTargetResponse> GetResponseAsync(string requestId);
		Task SendOnPremiseConnectorRequest(Guid linkId, IOnPremiseTargetRequest onPremiseTargetRequest);
		void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId);
		void RegisterOnPremise(RegistrationInformation registrationInformation);
		void UnregisterOnPremise(string connectionId);
		Task SendOnPremiseTargetResponse(Guid originId, IOnPremiseTargetResponse response);
	}
}
