using System;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal interface IRabbitMqAcknowledgeableChannel<TMessage> : IRabbitMqChannel<TMessage>
		where TMessage : class
	{
		IObservable<TMessage> OnReceived(bool autoAck);
		void Acknowledge(string acknowledgeId);
	}
}
