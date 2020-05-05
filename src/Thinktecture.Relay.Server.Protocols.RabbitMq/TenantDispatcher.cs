using System;
using System.Threading.Tasks;
using RabbitMQ.Client;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc cref="ITenantDispatcher{TRequest}" />
	public class TenantDispatcher<TRequest> : ITenantDispatcher<TRequest>, IDisposable
		where TRequest : IRelayClientRequest
	{
		private readonly IModel _model;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantDispatcher{TRequest}"/>.
		/// </summary>
		/// <param name="modelFactory">The <see cref="ModelFactory"/> to use.</param>
		public TenantDispatcher(ModelFactory modelFactory) => _model = modelFactory.Create();

		/// <inheritdoc />
		public Task DispatchRequestAsync(TRequest request)
			=> _model.PublishJsonAsync($"{Constants.RequestQueuePrefix}{request.TenantId}", request);

		/// <inheritdoc />
		public void Dispose() => _model.Dispose();
	}
}
