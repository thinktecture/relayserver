using System;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class TenantConnectorAdapterFactory<TRequest, TResponse> : ITenantConnectorAdapterFactory<TRequest>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> _hubContext;

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapterFactory{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="hubContext">An <see cref="IHubContext{THub}"/>.</param>
		public TenantConnectorAdapterFactory(IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> hubContext)
		{
			_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
		}

		/// <inheritdoc />
		public ITenantConnectorAdapter<TRequest> Create(Guid tenantId, string connectionId)
			=> new TenantConnectorAdapter<TRequest, TResponse>(tenantId, connectionId, _hubContext);
	}
}
