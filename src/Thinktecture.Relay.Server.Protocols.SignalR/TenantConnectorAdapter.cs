using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	public class TenantConnectorAdapter<TRequest, TResponse> : ITenantConnectorAdapter<TRequest>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> _hubContext;

		/// <inheritdoc />
		public Guid TenantId { get; }

		/// <inheritdoc />
		public string ConnectionId { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="hubContext">An <see cref="IHubContext{THub}"/>.</param>
		public TenantConnectorAdapter(Guid tenantId, string connectionId,
			IHubContext<ConnectorHub<TRequest, TResponse>, IConnector<TRequest>> hubContext)
		{
			_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));

			TenantId = tenantId;
			ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
		}

		/// <inheritdoc />
		public async Task RequestTargetAsync(TRequest request, CancellationToken cancellationToken = default)
			=> await _hubContext.Clients.Client(ConnectionId).RequestTarget(request);
	}
}
