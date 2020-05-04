using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqAcknowledgeChannel : RabbitMqChannelBase<IAcknowledgeRequest, AcknowledgeRequest>
	{
		public RabbitMqAcknowledgeChannel(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}
	}
}
