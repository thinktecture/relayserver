using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class TenantConnectorAdapterFactory<TRequest, TResponse> : ITenantConnectorAdapterFactory<TRequest>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapterFactory{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public TenantConnectorAdapterFactory(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		/// <inheritdoc />
		public ITenantConnectorAdapter<TRequest> Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<TenantConnectorAdapter<TRequest, TResponse>>(_serviceProvider, tenantId, connectionId);
	}
}
