using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <inheritdoc />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class TenantConnectorAdapter<TRequest, TResponse, TAcknowledge> : ITenantConnectorAdapter<TRequest>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly IHubContext<ConnectorHub<TRequest, TResponse, TAcknowledge>, IConnector<TRequest>> _hubContext;
		private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;

		/// <inheritdoc />
		public Guid TenantId { get; }

		/// <inheritdoc />
		public string ConnectionId { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantConnectorAdapter{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="hubContext">An <see cref="IHubContext{THub}"/>.</param>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
		public TenantConnectorAdapter(IHubContext<ConnectorHub<TRequest, TResponse, TAcknowledge>, IConnector<TRequest>> hubContext,
			Guid tenantId, string connectionId, IConnectionStatisticsWriter connectionStatisticsWriter)
		{
			_hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
			_connectionStatisticsWriter = connectionStatisticsWriter ?? throw new ArgumentNullException(nameof(connectionStatisticsWriter));

			ConnectionId = connectionId ?? throw new ArgumentNullException(nameof(connectionId));
			TenantId = tenantId;
		}

		/// <inheritdoc />
		public async Task RequestTargetAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			await _hubContext.Clients.Client(ConnectionId).RequestTarget(request);
			await _connectionStatisticsWriter.UpdateLastActivityTimeAsync(ConnectionId, cancellationToken);
		}
	}
}
