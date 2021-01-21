using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc />
	public class RabbitMqTenantHandlerFactory<TRequest, TResponse> : ITenantHandlerFactory<TRequest>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="RabbitMqTenantHandlerFactory{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public RabbitMqTenantHandlerFactory(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		/// <inheritdoc />
		public ITenantHandler<TRequest> Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<RabbitMqTenantHandler<TRequest, TResponse>>(_serviceProvider, tenantId, connectionId);
	}
}
