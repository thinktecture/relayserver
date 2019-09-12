using System;
using System.Threading.Tasks;

namespace Thinktecture.Relay.Server.Communication.RabbitMq
{
	internal interface IRabbitMqChannel<TMessage> : IDisposable
		where TMessage : class
	{
		IObservable<TMessage> OnReceived();
		Task Dispatch(TMessage message);
	}
}
