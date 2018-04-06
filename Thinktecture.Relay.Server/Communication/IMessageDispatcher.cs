using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IMessageDispatcher
	{
		IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool autoAck);
		IObservable<IOnPremiseConnectorResponse> OnResponseReceived();
		IObservable<string> OnAcknowledgeReceived();

		void AcknowledgeRequest(string acknowledgeId);

		void DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request);
		void DispatchResponse(Guid originId, IOnPremiseConnectorResponse response);
		void DispatchAcknowledge(Guid originId, string acknowledgeId);
	}
}
