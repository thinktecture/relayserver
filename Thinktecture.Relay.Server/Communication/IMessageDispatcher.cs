using System;
using System.Threading.Tasks;
using Thinktecture.Relay.OnPremiseConnector.OnPremiseTarget;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IMessageDispatcher
	{
		IObservable<IOnPremiseTargetRequest> OnRequestReceived(string onPremiseId, string connectionId, bool noAck);
		IObservable<IOnPremiseTargetResponse> OnResponseReceived(string originId);

		void AcknowledgeRequest(string onPremiseId, string acknowledgeId);

		Task DispatchRequest(string onPremiseId, IOnPremiseTargetRequest request);
		Task DispatchResponse(string originId, IOnPremiseTargetResponse response);
	}
}
