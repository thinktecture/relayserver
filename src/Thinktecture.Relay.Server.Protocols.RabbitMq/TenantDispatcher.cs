using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantDispatcher{TRequest}" />
	public class TenantDispatcher<TRequest> : ITenantDispatcher<TRequest>, IDisposable
		where TRequest : IClientRequest
	{
		private readonly ILogger<TenantDispatcher<TRequest>> _logger;
		private readonly IModel _model;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="TenantDispatcher{TRequest}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public TenantDispatcher(ILogger<TenantDispatcher<TRequest>> logger, ModelFactory modelFactory, IOptions<RabbitMqOptions> options)
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
		public async Task DispatchRequestAsync(TRequest request)
		{
			_logger.LogTrace("Sending request {@Request}", request);
			await _model.PublishJsonAsync($"{Constants.RequestQueuePrefix}{request.TenantId}", request, autoDelete: false);
			_logger.LogDebug("Sent request {RequestId} to tenant {TenantId}", request.RequestId, request.TenantId);
		}

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
