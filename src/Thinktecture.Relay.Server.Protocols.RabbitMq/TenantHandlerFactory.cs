using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.RabbitMq
{
	/// <inheritdoc />
	public class TenantHandlerFactory<TRequest, TResponse> : ITenantHandlerFactory<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantHandlerFactory{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public TenantHandlerFactory(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		}

		/// <inheritdoc />
		public ITenantHandler<TRequest> Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<TenantHandler<TRequest, TResponse>>(_serviceProvider, tenantId, connectionId);
	}
}
