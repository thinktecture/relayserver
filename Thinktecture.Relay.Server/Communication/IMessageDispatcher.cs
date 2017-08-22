using System;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IMessageDispatcher
	{
		IObservable<IOnPremiseTargetRequest> OnRequestReceived(Guid linkId, string connectionId, bool noAck);
		IObservable<IOnPremiseTargetResponse> OnResponseReceived(Guid originId);

		void AcknowledgeRequest(Guid linkId, string acknowledgeId);

		Task DispatchRequest(Guid linkId, IOnPremiseTargetRequest request);
		Task DispatchResponse(Guid originId, IOnPremiseTargetResponse response);
	}
}
