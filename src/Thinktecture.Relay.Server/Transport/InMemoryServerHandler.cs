using System;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	internal class InMemoryServerHandler<TResponse> : IServerHandler<TResponse>
		where TResponse : IRelayTargetResponse
	{
		public event AsyncEventHandler<TResponse> ResponseReceived;
		public event AsyncEventHandler<IAcknowledgeRequest> AcknowledgeReceived;

		public InMemoryServerHandler(IServerDispatcher<TResponse> serverDispatcher)
		{
			switch (serverDispatcher)
			{
				case null:
					throw new ArgumentNullException(nameof(serverDispatcher));

				case InMemoryServerDispatcher<TResponse> inMemoryServerDispatcher:
					inMemoryServerDispatcher.ResponseReceived += async (sender, @event) => await ResponseReceived.InvokeAsync(sender, @event);
					inMemoryServerDispatcher.AcknowledgeReceived
						+= async (sender, @event) => await AcknowledgeReceived.InvokeAsync(sender, @event);
					break;

				default:
					throw new ArgumentException(
						$"The registered server dispatcher must be of type {nameof(InMemoryServerDispatcher<TResponse>)}",
						nameof(serverDispatcher));
			}
		}
	}
}
