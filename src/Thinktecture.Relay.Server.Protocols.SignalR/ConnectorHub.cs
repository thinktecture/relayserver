using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <summary>
	/// A strongly-typed interface for a <see cref="Hub{T}"/>.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface IConnector<in TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// A strongly-typed method for a <see cref="Hub{T}"/>.
		/// </summary>
		/// <param name="request">An <see cref="IRelayClientRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTarget(TRequest request);
	}

	/// <inheritdoc cref="IConnectorTransport{TResponse}" />
	// ReSharper disable once ClassNeverInstantiated.Global
	public class ConnectorHub<TRequest, TResponse> : Hub<IConnector<TRequest>>, IConnectorTransport<TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly ITenantConnectorAdapterRegistry<TRequest> _tenantConnectorAdapterRegistry;
		private readonly IServerDispatcher<TResponse> _serverDispatcher;

		/// <summary>
		/// Initializes a new instance of <see cref="ConnectorHub{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantConnectorAdapterRegistry">An <see cref="ITenantConnectorAdapterRegistry{TRequest}"/>.</param>
		/// <param name="serverDispatcher">An <see cref="IServerDispatcher{TResponse}"/>.</param>
		public ConnectorHub(ITenantConnectorAdapterRegistry<TRequest> tenantConnectorAdapterRegistry,
			IServerDispatcher<TResponse> serverDispatcher)
		{
			_tenantConnectorAdapterRegistry = tenantConnectorAdapterRegistry
				?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
			_serverDispatcher = serverDispatcher ?? throw new ArgumentNullException(nameof(serverDispatcher));
		}

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; } = 64 * 1024; // 64kb

		/// <inheritdoc />
		public override async Task OnConnectedAsync()
		{
			await _tenantConnectorAdapterRegistry.RegisterAsync(Guid.Empty, Context.ConnectionId); // TODO get tenant id
			await base.OnConnectedAsync();
		}

		/// <inheritdoc />
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await _tenantConnectorAdapterRegistry.UnregisterAsync(Context.ConnectionId);
			await base.OnDisconnectedAsync(exception);
		}

		/// <inheritdoc />
		[HubMethodName("Deliver")]
		public async Task DeliverAsync(TResponse response) => await _serverDispatcher.DispatchResponseAsync(response);

		/// <inheritdoc />
		[HubMethodName("Acknowledge")]
		public async Task AcknowledgeAsync(IAcknowledgeRequest request) => await _serverDispatcher.DispatchAcknowledgeAsync(request);

		/// <inheritdoc />
		[HubMethodName("Pong")]
		public Task PongAsync() => throw new NotImplementedException();
	}
}
