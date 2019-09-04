using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqResponseChannel : RabbitMqChannelBase<IOnPremiseConnectorResponse>
	{
		public RabbitMqResponseChannel(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}

		public override IObservable<IOnPremiseConnectorResponse> OnReceived()
		{
			return CreateObservable<OnPremiseConnectorResponse>();
		}

		public override Task Dispatch(IOnPremiseConnectorResponse message)
		{
			var data = Serialize(message, out var properties);
			Send(data, properties);

			return Task.CompletedTask;
		}
	}
}
