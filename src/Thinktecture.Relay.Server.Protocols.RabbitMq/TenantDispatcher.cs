using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantDispatcher{TRequest}" />
	public class TenantDispatcher<TRequest> : ITenantDispatcher<TRequest>, IDisposable
		where TRequest : IRelayClientRequest
	{
		private readonly IModel _model;

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="TenantDispatcher{TRequest}"/>.
		/// </summary>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="options">An <see cref="IOptions{TOptions}"/>.</param>
		public TenantDispatcher(ModelFactory modelFactory, IOptions<RabbitMqOptions> options)
		{
			if (modelFactory == null)
			{
				throw new ArgumentNullException(nameof(modelFactory));
			}

			_model = modelFactory.Create();
			BinarySizeThreshold = options.Value.MaximumBinarySize;
		}

		/// <inheritdoc />
		public Task DispatchRequestAsync(TRequest request)
			=> _model.PublishJsonAsync($"{Constants.RequestQueuePrefix}{request.TenantId}", request, autoDelete: false);

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
