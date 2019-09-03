using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqResponseChannelBase : RabbitMqChannelBase<IOnPremiseConnectorResponse>
	{
		public RabbitMqResponseChannelBase(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
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

			lock (Model)
			{
				DeclareAndBind();
				Model.BasicPublish(Exchange, ChannelId, false, properties, data);
			}

			return Task.CompletedTask;
		}
	}
}
