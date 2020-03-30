using System;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal interface IRabbitMqRequestChannel : IRabbitMqChannel<IOnPremiseConnectorRequest>
	{
		void Acknowledge(string acknowledgeId);
		IObservable<IOnPremiseConnectorRequest> OnReceived(bool autoAck);
	}
}
