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
	public class RabbitMqServerDispatcher<TResponse> : IServerDispatcher<TResponse>, IDisposable
		where TResponse : ITargetResponse
	{
		private readonly ILogger<RabbitMqServerDispatcher<TResponse>> _logger;
		private readonly IModel _responseModel;
		private readonly IModel _acknowledgeModel;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="RabbitMqServerDispatcher{TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgegory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="rabbitMqOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public RabbitMqServerDispatcher(ILogger<RabbitMqServerDispatcher<TResponse>> logger, ModelFactory modelFactory,
			IOptions<RabbitMqOptions> rabbitMqOptions)
		{
			if (modelFactory == null) throw new ArgumentNullException(nameof(modelFactory));
			if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			BinarySizeThreshold = rabbitMqOptions.Value.MaximumBinarySize;

			_responseModel = modelFactory.Create("response dispatcher");
			_acknowledgeModel = modelFactory.Create("acknowledge dispatcher");
		}

		/// <inheritdoc />
		public async Task DispatchResponseAsync(TResponse response)
		{
			await _responseModel.PublishJsonAsync($"{Constants.ResponseQueuePrefix}{response.RequestOriginId}", response, durable: false,
				persistent: false);
			_logger.LogDebug("Dispatched response for request {RequestId} to origin {OriginId}", response.RequestId, response.RequestOriginId);
		}

		/// <inheritdoc />
		public async Task DispatchAcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogTrace("Dispatching acknowledge {@AcknowledgeRequest}", request);
			await _acknowledgeModel.PublishJsonAsync($"{Constants.AcknowledgeQueuePrefix}{request.OriginId}", request, durable: false,
				persistent: false);
			_logger.LogDebug("Dispatched acknowledgement for request {RequestId} to origin {OriginId}", request.RequestId, request.OriginId);
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_responseModel.Dispose();
			_acknowledgeModel.Dispose();
		}
	}
}
