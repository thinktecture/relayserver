using System;
using Thinktecture.Relay.Server.Communication.RabbitMq;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication
{
	public interface IMessageDispatcher : IDisposable
	{
		IObservable<IOnPremiseConnectorRequest> OnRequestReceived(Guid linkId, string connectionId, bool autoAck);
		IObservable<IOnPremiseConnectorResponse> OnResponseReceived();
		IObservable<IAcknowledgeRequest> OnAcknowledgeReceived();

		void AcknowledgeRequest(Guid linkId, string acknowledgeId);

		void DispatchRequest(Guid linkId, IOnPremiseConnectorRequest request);
		void DispatchResponse(Guid originId, IOnPremiseConnectorResponse response);
		void DispatchAcknowledge(Guid originId, string connectionId, string acknowledgeId);
	}
}
