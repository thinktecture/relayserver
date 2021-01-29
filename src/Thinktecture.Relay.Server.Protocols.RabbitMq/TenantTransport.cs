using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantTransport{T}" />
	public class TenantTransport<T> : ITenantTransport<T>, IDisposable
		where T : IClientRequest
	{
		private readonly ILogger<TenantTransport<T>> _logger;
		private readonly IModel _model;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantTransport{T}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCatgeory}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="rabbitMqOptions">An <see cref="IOptions{TOptions}"/>.</param>
		public TenantTransport(ILogger<TenantTransport<T>> logger, ModelFactory modelFactory, IOptions<RabbitMqOptions> rabbitMqOptions)
		{
			if (modelFactory == null) throw new ArgumentNullException(nameof(modelFactory));
			if (rabbitMqOptions == null) throw new ArgumentNullException(nameof(rabbitMqOptions));

			_logger = logger ?? throw new ArgumentNullException(nameof(logger));

			BinarySizeThreshold = rabbitMqOptions.Value.MaximumBinarySize;

			_model = modelFactory.Create("tenant dispatcher");
		}

		/// <inheritdoc />
		public async Task TransportAsync(T request)
		{
			try
			{
				await _model.PublishJsonAsync($"{Constants.RequestQueuePrefix}{request.TenantId}", request, autoDelete: false);
				_logger.LogDebug("Published request {RequestId} to tenant {TenantId}", request.RequestId, request.TenantId);
			}
			catch (RabbitMQClientException ex)
			{
				_logger.LogError(ex, "An error occured while dispatching request {RequestId} to tenant {TenantId} queue", request.RequestId,
					request.TenantId);
				throw new TransportException(ex);
			}
		}

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
