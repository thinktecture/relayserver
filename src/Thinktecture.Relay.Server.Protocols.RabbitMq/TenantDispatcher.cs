using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
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
			BinarySizeThreshold = options?.Value.MaximumBinarySize ?? throw new ArgumentNullException(nameof(options));
			_model = modelFactory?.Create() ?? throw new ArgumentNullException(nameof(modelFactory));
		}

		/// <inheritdoc />
		public async Task DispatchRequestAsync(TRequest request)
		{
			try
			{
				await _model.PublishJsonAsync($"{Constants.RequestQueuePrefix}{request.TenantId}", request, autoDelete: false);
				_logger.LogDebug("Sent request {RequestId} to tenant {TenantId}", request.RequestId, request.TenantId);
			}
			catch (RabbitMQClientException ex)
			{
				_logger.LogError(ex, "An error occured while dispatching request {RequestId} to tenant {TenantId} queue", request.RequestId, request.TenantId);
				throw new TransportException(ex);
			}
		}

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
