using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IBackendCommunication
	{
		string OriginId { get; }
		Task<IOnPremiseTargetResponse> GetResponseAsync(string requestId);
		Task SendOnPremiseConnectorRequest(string linkId, IOnPremiseTargetRequest onPremiseTargetRequest);
		void AcknowledgeOnPremiseConnectorRequest(string connectionId, string acknowledgeId);
		void RegisterOnPremise(RegistrationInformation registrationInformation);
		void UnregisterOnPremise(string connectionId);
		Task SendOnPremiseTargetResponse(string originId, IOnPremiseTargetResponse response);
	}
}
