using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
		private readonly ILogger<ServerDispatcher<TResponse>> _logger;
		private readonly IModel _model;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="ServerDispatcher{TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgegory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public ServerDispatcher(ILogger<ServerDispatcher<TResponse>> logger, ModelFactory modelFactory, IOptions<RabbitMqOptions> options)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_model = modelFactory.Create();
			BinarySizeThreshold = options.Value.MaximumBinarySize;
		}

		/// <inheritdoc />
		public async Task DispatchResponseAsync(TResponse response)
		{
			_logger.LogTrace("Dispatching response {@Response} to other server");
			await _model.PublishJsonAsync($"{Constants.ResponseQueuePrefix}{response.RequestOriginId}", response);

			_logger.LogDebug("Response for request {RequestId} was dispatched to server {OriginId}", response.RequestId, response.RequestOriginId);
		}

		/// <inheritdoc />
		public async Task DispatchAcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Dispatching acknowledge {@AcknowledgeRequest} to other server");
			await _model.PublishJsonAsync($"{Constants.AcknowledgeQueuePrefix}{request.OriginId}", request);

			_logger.LogDebug("Acknowledge request {AcknowledgeId} was dispatched to server {OriginId}", request.AcknowledgeId, request.OriginId);
		}

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
