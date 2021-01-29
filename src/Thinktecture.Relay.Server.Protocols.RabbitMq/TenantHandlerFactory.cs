using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc />
	public class TenantHandlerFactory<TRequest, TAcknowledge> : ITenantHandlerFactory
		where TRequest : IClientRequest
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantHandlerFactory{TRequest,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public TenantHandlerFactory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

		/// <inheritdoc />
		public ITenantHandler Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<TenantHandler<TRequest, TAcknowledge>>(_serviceProvider, tenantId, connectionId);
	}
}
