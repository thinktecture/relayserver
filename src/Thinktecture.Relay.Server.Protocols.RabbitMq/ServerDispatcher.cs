using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="IServerDispatcher{TResponse}" />
	public class ServerDispatcher<TResponse> : IServerDispatcher<TResponse>, IDisposable
		where TResponse : ITargetResponse
	{
		private readonly IModel _model;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ServerDispatcher{TResponse}"/>.
		/// </summary>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public ServerDispatcher(ModelFactory modelFactory, IOptions<RabbitMqOptions> options)
		{
			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_model = modelFactory.Create();
			BinarySizeThreshold = options.Value.MaximumBinarySize;
		}

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
