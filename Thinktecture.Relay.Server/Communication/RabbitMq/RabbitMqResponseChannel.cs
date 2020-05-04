using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqResponseChannel : RabbitMqChannelBase<IOnPremiseConnectorResponse, OnPremiseConnectorResponse>
	{
		public RabbitMqResponseChannel(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}
	}
}
