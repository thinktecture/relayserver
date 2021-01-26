using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Persistence;
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
		/// A strongly-typed method for a <see cref="Hub{T}"/> to request a connector's target.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTarget(TRequest request);

		/// <summary>
		/// A strongly-typed method for a <see cref="Hub{T}"/> to update a connector's run-time config.
		/// </summary>
		/// <param name="config">An <see cref="ITenantConfig"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task Configure(ITenantConfig config);
	}

	/// <inheritdoc cref="Hub{T}" />
	[Authorize(Constants.DefaultAuthenticationPolicy)]
	public class ConnectorHub<TRequest, TResponse> : Hub<IConnector<TRequest>>, IConnectorTransport<TResponse>
		where TRequest : IClientRequest
		where TResponse : class, ITargetResponse
	{
		private readonly ILogger<ConnectorHub<TRequest, TResponse>> _logger;
		private readonly IAcknowledgeCoordinator _acknowledgeCoordinator;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;
		private readonly RelayServerContext _relayServerContext;
		private readonly ITenantRepository _tenantRepository;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectorHub{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="responseCoordinator">An <see cref="IResponseCoordinator{TResponse}"/>.</param>
		/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
		/// <param name="relayServerContext">An <see cref="RelayServerContext"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		public ConnectorHub(ILogger<ConnectorHub<TRequest, TResponse>> logger, IAcknowledgeCoordinator acknowledgeCoordinator,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry,
			IResponseCoordinator<TResponse> responseCoordinator, IConnectionStatisticsWriter connectionStatisticsWriter,
			RelayServerContext relayServerContext, ITenantRepository tenantRepository)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_connectionStatisticsWriter = connectionStatisticsWriter ?? throw new ArgumentNullException(nameof(connectionStatisticsWriter));
			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
		}

		/// <inheritdoc />
		public override async Task OnConnectedAsync()
		{
			var tenant = Context.User.GetTenantInfo();
			_logger.LogDebug("Connection {ConnectionId} incoming for tenant {@Tenant}", Context.ConnectionId, tenant);

			await _tenantConnectorAdapterRegistry.RegisterAsync(tenant.Id, Context.ConnectionId);
			await _connectionStatisticsWriter.SetConnectionTimeAsync(Context.ConnectionId, tenant.Id, _relayServerContext.OriginId,
				Context.GetHttpContext().Connection.RemoteIpAddress);

			var config = await _tenantRepository.LoadTenantConfigAsync(tenant.Id);
			if (config != null)
			{
				await Clients.Caller.Configure(config);
			}

			await base.OnConnectedAsync();
		}

		/// <inheritdoc />
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			_logger.LogDebug("Connection {ConnectionId} disconnected for tenant {@Tenant}", Context.ConnectionId,
				Context.User.GetTenantInfo());

			await _tenantConnectorAdapterRegistry.UnregisterAsync(Context.ConnectionId);
			await _connectionStatisticsWriter.SetDisconnectTimeAsync(Context.ConnectionId);

			await base.OnDisconnectedAsync(exception);
		}

		/// <inheritdoc />
		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 16 * 1024; // 16kb

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.DeliverAsync"/>
		[HubMethodName("Deliver")]
		public async Task DeliverAsync(TResponse response)
		{
			_logger.LogDebug("Connection {ConnectionId} received response for request {RequestId}", Context.ConnectionId, response.RequestId);

			await _responseCoordinator.ProcessResponseAsync(response);
			await _connectionStatisticsWriter.UpdateLastActivityTimeAsync(Context.ConnectionId);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="request">An <see cref="IAcknowledgeRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.AcknowledgeAsync"/>
		[HubMethodName("Acknowledge")]
		public async Task AcknowledgeAsync(IAcknowledgeRequest request)
		{
			_logger.LogDebug("Connection {ConnectionId} received acknowledgment for request {RequestId}", Context.ConnectionId,
				request.RequestId);

			await _acknowledgeCoordinator.AcknowledgeRequestAsync(request);
			await _connectionStatisticsWriter.UpdateLastActivityTimeAsync(Context.ConnectionId);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.PongAsync"/>
		[HubMethodName("Pong")]
		public Task PongAsync() => throw new NotImplementedException();
	}
}
