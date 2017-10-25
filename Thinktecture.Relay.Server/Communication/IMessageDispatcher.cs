using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IMessageDispatcher
	{
		IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool noAck);
		IObservable<IOnPremiseConnectorResponse> OnResponseReceived(Guid originId);

		void AcknowledgeRequest(Guid linkId, string acknowledgeId);

		Task DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request);
		Task DispatchResponse(Guid originId, IOnPremiseConnectorResponse response);
	}
}
