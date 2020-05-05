using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="IServerDispatcher{TResponse}" />
	public class ServerDispatcher<TResponse> : IServerDispatcher<TResponse>, IDisposable
		where TResponse : IRelayTargetResponse
	{
		private readonly IModel _model;

		/// <summary>
		/// Initializes a new instance of <see cref="ServerDispatcher{TResponse}"/>.
		/// </summary>
		/// <param name="modelFactory">The <see cref="ModelFactory"/> to use.</param>
		public ServerDispatcher(ModelFactory modelFactory) => _model = modelFactory.Create();

		/// <inheritdoc />
		public Task DispatchResponseAsync(TResponse response)
			=> _model.PublishJsonAsync($"{Constants.ResponseQueuePrefix}{response.RequestOriginId}", response);

		/// <inheritdoc />
		public Task DispatchAcknowledgeAsync(IAcknowledgeRequest request)
			=> _model.PublishJsonAsync($"{Constants.AcknowledgeQueuePrefix}{request.OriginId}", request);

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
