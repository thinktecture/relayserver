using System;
using System.Globalization;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Serilog;
using Thinktecture.Relay.Server.Config;
using Thinktecture.Relay.Server.OnPremise;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal class RabbitMqRequestChannel : RabbitMqChannelBase<IOnPremiseConnectorRequest>, IRabbitMqAcknowledgeableChannel<IOnPremiseConnectorRequest>
	{
		public RabbitMqRequestChannel(ILogger logger, IConnection connection, IConfiguration configuration, string exchange, string channelId, string queuePrefix)
			: base(logger, connection, configuration, exchange, channelId, queuePrefix)
		{
		}

		public override Task Dispatch(IOnPremiseConnectorRequest message)
		{
			var data = Serialize(message, out var properties);

			if (message.Expiration != TimeSpan.Zero)
			{
				Logger?.Verbose("Setting RabbitMQ message TTL. request-id={RequestId}, request-expiration={RequestExpiration}", message.RequestId, message.Expiration);
				properties.Expiration = message.Expiration.TotalMilliseconds.ToString(CultureInfo.InvariantCulture);
			}

			Send(data, properties);

			return Task.CompletedTask;
		}


		public override IObservable<IOnPremiseConnectorRequest> OnReceived()
		{
			return OnReceived(true);
		}

		public IObservable<IOnPremiseConnectorRequest> OnReceived(bool autoAck)
		{
			return CreateObservable<OnPremiseConnectorRequest>(autoAck, (request, deliveryTag) =>
			{
				if (autoAck)
				{
					return;
				}

				switch (request.AcknowledgmentMode)
				{
					case AcknowledgmentMode.Auto:
						lock (Model)
						{
							Model.BasicAck(deliveryTag, false);
						}
						Logger?.Debug("Request was automatically acknowledged. request-id={RequestId}", request.RequestId);
						break;

					case AcknowledgmentMode.Default:
					case AcknowledgmentMode.Manual:
						request.AcknowledgeId = deliveryTag.ToString();
						request.AcknowledgeOriginId = new Guid(ChannelId);
						Logger?.Verbose("Request acknowledge id was set. request-id={RequestId}, acknowledge-id={AcknowledgeId}", request.RequestId, request.AcknowledgeId);
						break;
				}
			});
		}

		public void Acknowledge(string acknowledgeId)
		{
			if (UInt64.TryParse(acknowledgeId, out var deliveryTag))
			{
				lock (Model)
				{
					Model.BasicAck(deliveryTag, false);
				}
			}
		}
	}
}
