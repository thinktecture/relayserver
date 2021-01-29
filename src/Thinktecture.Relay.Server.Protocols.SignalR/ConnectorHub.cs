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
	/// <typeparam name="T">The type of request.</typeparam>
	public interface IConnector<in T>
		where T : IClientRequest
	{
		/// <summary>
		/// A strongly-typed method for a <see cref="Hub{T}"/> to request a connector's target.
		/// </summary>
		/// <param name="request">An <see cref="IClientRequest"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTarget(T request);

		/// <summary>
		/// A strongly-typed method for a <see cref="Hub{T}"/> to update a connector's run-time config.
		/// </summary>
		/// <param name="config">An <see cref="ITenantConfig"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task Configure(ITenantConfig config);
	}

	/// <inheritdoc />
	[Authorize(Constants.DefaultAuthenticationPolicy)]
	// ReSharper disable once ClassNeverInstantiated.Global
	public class ConnectorHub<TRequest, TResponse, TAcknowledge> : Hub<IConnector<TRequest>>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
		where TAcknowledge : IAcknowledgeRequest
	{
		private readonly ILogger<ConnectorHub<TRequest, TResponse, TAcknowledge>> _logger;
		private readonly ITenantRepository _tenantRepository;
		private readonly IResponseDispatcher<TResponse> _responseDispatcher;
		private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
		private readonly ConnectorRegistry<TRequest> _connectorRegistry;
		private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectorHub{TRequest,TResponse,TAcknowledge}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="tenantRepository">An <see cref="ITenantRepository"/>.</param>
		/// <param name="responseDispatcher">An <see cref="IResponseDispatcher{T}"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
		/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
		/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
		public ConnectorHub(ILogger<ConnectorHub<TRequest, TResponse, TAcknowledge>> logger, ITenantRepository tenantRepository,
			IResponseDispatcher<TResponse> responseDispatcher, IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator,
			ConnectorRegistry<TRequest> connectorRegistry, IConnectionStatisticsWriter connectionStatisticsWriter)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
			_responseDispatcher = responseDispatcher ?? throw new ArgumentNullException(nameof(responseDispatcher));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
			_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
			_connectionStatisticsWriter = connectionStatisticsWriter ?? throw new ArgumentNullException(nameof(connectionStatisticsWriter));
		}

		/// <inheritdoc />
		public override async Task OnConnectedAsync()
		{
			var tenant = Context.User.GetTenantInfo();
			_logger.LogDebug("Connection {ConnectionId} incoming for tenant {@Tenant}", Context.ConnectionId, tenant);

			await _connectorRegistry.RegisterAsync(Context.ConnectionId, tenant.Id, Context.GetHttpContext().Connection.RemoteIpAddress);

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

			await _connectorRegistry.UnregisterAsync(Context.ConnectionId);

			await base.OnDisconnectedAsync(exception);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		[HubMethodName("Deliver")]
		// ReSharper disable once UnusedMember.Global
		public async Task DeliverAsync(TResponse response)
		{
			_logger.LogDebug("Connection {ConnectionId} received response for request {RequestId}", Context.ConnectionId, response.RequestId);

			response.ConnectionId = Context.ConnectionId;

			await _responseDispatcher.DispatchAsync(response);
			await _connectionStatisticsWriter.UpdateLastActivityTimeAsync(Context.ConnectionId);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="request">The acknowledge request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		[HubMethodName("Acknowledge")]
		// ReSharper disable once UnusedMember.Global
		public async Task AcknowledgeAsync(TAcknowledge request)
		{
			_logger.LogDebug("Connection {ConnectionId} received acknowledgement for request {RequestId}", Context.ConnectionId,
				request.RequestId);

			await _acknowledgeCoordinator.ProcessAcknowledgeAsync(request);
			await _connectionStatisticsWriter.UpdateLastActivityTimeAsync(Context.ConnectionId);
		}

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		[HubMethodName("Pong")]
		// ReSharper disable once UnusedMember.Global
		public Task PongAsync() => throw new NotImplementedException();
	}
}
