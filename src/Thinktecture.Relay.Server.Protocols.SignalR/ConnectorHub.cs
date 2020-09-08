using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR
{
	/// <summary>
	/// A strongly-typed interface for a <see cref="Hub{T}"/>.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface IConnector<in TRequest>
		where TRequest : IClientRequest
	{
		/// <summary>
		/// A strongly-typed method for a <see cref="Hub{T}"/>.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTarget(TRequest request);
	}

	/// <inheritdoc cref="Hub{T}" />
	[Authorize(Constants.DefaultAuthenticationPolicy)]
	public class ConnectorHub<TRequest, TResponse> : Hub<IConnector<TRequest>>, IConnectorTransport<TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<ConnectorHub<TRequest, TResponse>> _logger;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;
		private readonly AcknowledgeCoordinator<TRequest, TResponse> _acknowledgeCoordinator;
		private readonly IServerDispatcher<TResponse> _serverDispatcher;

		/// <summary>
		/// Initializes a new instance of <see cref="ConnectorHub{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="serverDispatcher">An <see cref="IServerDispatcher{TResponse}"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="acknowledgeCoordinator">The <see cref="AcknowledgeCoordinator{TRequest,TResponse}"/>.</param>
		public ConnectorHub(ILogger<ConnectorHub<TRequest, TResponse>> logger, IServerDispatcher<TResponse> serverDispatcher,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry,
			AcknowledgeCoordinator<TRequest, TResponse> acknowledgeCoordinator)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_serverDispatcher = serverDispatcher ?? throw new ArgumentNullException(nameof(serverDispatcher));
			_tenantConnectorAdapterRegistry = tenantConnectorAdapterRegistry
				?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
		}

		/// <inheritdoc />
		public int? BinarySizeThreshold { get; } = 64 * 1024; // 64kb

		/// <inheritdoc />
		public override async Task OnConnectedAsync()
		{
			var tenant = Context.User.GetTenantInfo();
			_logger.LogDebug("Connection incoming for tenant {@Tenant}", tenant);

			await _tenantConnectorAdapterRegistry.RegisterAsync(tenant.Id, Context.ConnectionId);
			await base.OnConnectedAsync();
		}

		/// <inheritdoc />
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			await _tenantConnectorAdapterRegistry.UnregisterAsync(Context.ConnectionId);
			_logger.LogDebug("Connection disconnected for tenant {@Tenant}", Context.User.GetTenantInfo());

			await base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.DeliverAsync"/>
		[HubMethodName("Deliver")]
		public async Task DeliverAsync(TResponse response) => await _serverDispatcher.DispatchResponseAsync(response);

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="request">An <see cref="IAcknowledgeRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.AcknowledgeAsync"/>
		[HubMethodName("Acknowledge")]
		public async Task AcknowledgeAsync(IAcknowledgeRequest request) => await _acknowledgeCoordinator.AcknowledgeRequestAsync(request);

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.PongAsync"/>
		[HubMethodName("Pong")]
		public Task PongAsync() => throw new NotImplementedException();
	}
}
