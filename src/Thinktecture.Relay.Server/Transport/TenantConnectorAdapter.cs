using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc cref="ITenantConnectorAdapter{TRequest,TResponse}" />
	public class TenantConnectorAdapter<TRequest, TResponse> : ITenantConnectorAdapter<TRequest, TResponse>, IDisposable
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly IConnectorTransport<TRequest> _connectorTransport;
		private readonly ITenantHandler<TRequest, TResponse> _tenantHandler;

		/// <inheritdoc />
		public Guid TenantId { get; }

		/// <inheritdoc />
		public string ConnectionId { get; }

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="connectorTransport">An <see cref="IConnectorTransport{TRequest}"/>.</param>
		/// <param name="tenantHandlerFactory">An <see cref="ITenantHandlerFactory{TRequest,TResponse}"/>.</param>
		public TenantConnectorAdapter(Guid tenantId, string connectionId, IConnectorTransport<TRequest> connectorTransport,
			ITenantHandlerFactory<TRequest, TResponse> tenantHandlerFactory)
		{
			_connectorTransport = connectorTransport;

			_tenantHandler = tenantHandlerFactory.Create(tenantId);
			_tenantHandler.RequestReceived += OnRequestReceived;

			TenantId = tenantId;
			ConnectionId = connectionId;
		}

		/// <inheritdoc />
		public void Dispose()
		{
			_tenantHandler.RequestReceived -= OnRequestReceived;
			(_tenantHandler as IDisposable)?.Dispose();
		}

		/// <inheritdoc />
		public Task AcknowledgeRequestAsync(string acknowledgeId)
		{
			throw new System.NotImplementedException();
		}

		private async Task OnRequestReceived(object sender, TRequest @event)
			=> await _connectorTransport.RequestTargetAsync(@event, ConnectionId);
	}
}
