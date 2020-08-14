using System;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc />
	public class TenantHandlerFactory<TRequest, TResponse> : ITenantHandlerFactory<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IServerHandler<TResponse> _serverHandler;
		private readonly ModelFactory _modelFactory;
		private readonly RelayServerContext _relayServerContext;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantHandlerFactory{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serverHandler">An <see cref="IServerHandler{TResponse}"/>.</param>
		/// <param name="modelFactory">The <see cref="ModelFactory"/>.</param>
		/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
		public TenantHandlerFactory(IServerHandler<TResponse> serverHandler, ModelFactory modelFactory, RelayServerContext relayServerContext)
		{
			_serverHandler = serverHandler ?? throw new ArgumentNullException(nameof(serverHandler));
			_modelFactory = modelFactory ?? throw new ArgumentNullException(nameof(modelFactory));
			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
		}

		/// <inheritdoc />
		public ITenantHandler<TRequest> Create(Guid tenantId)
			=> new TenantHandler<TRequest, TResponse>(tenantId, _serverHandler, _modelFactory, _relayServerContext);
	}
}
