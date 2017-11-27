using RabbitMQ.Client;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal interface IRabbitMqFactory
	{
		IConnection CreateConnection();
	}
}
