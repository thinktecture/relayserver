using System;
using Microsoft.Extensions.DependencyInjection;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class SignalRTenantConnectorAdapterFactory<TRequest, TResponse> : ITenantConnectorAdapterFactory<TRequest>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly IServiceProvider _serviceProvider;

		/// <summary>
		/// Initializes a new instance of the <see cref="SignalRTenantConnectorAdapterFactory{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="serviceProvider">An <see cref="IServiceProvider"/>.</param>
		public SignalRTenantConnectorAdapterFactory(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

		/// <inheritdoc />
		public ITenantConnectorAdapter<TRequest> Create(Guid tenantId, string connectionId)
			=> ActivatorUtilities.CreateInstance<SignalRTenantConnectorAdapter<TRequest, TResponse>>(_serviceProvider, tenantId, connectionId);
	}
}
