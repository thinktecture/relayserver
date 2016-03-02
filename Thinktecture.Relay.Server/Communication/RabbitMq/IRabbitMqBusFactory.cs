using EasyNetQ;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal interface IRabbitMqBusFactory
	{
		IBus CreateBus();
	}
}