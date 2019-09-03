using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqAcknowledgeChannel : RabbitMqChannelBase<string>
	{
		public RabbitMqAcknowledgeChannel(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}

		public override IObservable<string> OnReceived()
		{
			return CreateObservable<string>();
		}

		public override Task Dispatch(string message)
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
