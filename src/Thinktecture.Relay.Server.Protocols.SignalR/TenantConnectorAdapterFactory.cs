using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class TenantConnectorAdapterFactory<TRequest, TResponse, TAcknowledge> : ITenantConnectorAdapterFactory<TRequest>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantConnectorAdapterFactory{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public TenantConnectorAdapterFactory(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		/// <inheritdoc />
		public ITenantConnectorAdapter<TRequest> Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<TenantConnectorAdapter<TRequest, TResponse, TAcknowledge>>(_serviceProvider, tenantId,
				connectionId);
	}
}
