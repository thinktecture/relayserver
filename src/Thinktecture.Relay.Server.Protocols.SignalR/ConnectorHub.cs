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
		where TResponse : class, ITargetResponse
	{
		private readonly ILogger<ConnectorHub<TRequest, TResponse>> _logger;
		private readonly IAcknowledgeCoordinator _acknowledgeCoordinator;
		private readonly TenantConnectorAdapterRegistry<TRequest, TResponse> _tenantConnectorAdapterRegistry;
		private readonly IResponseCoordinator<TResponse> _responseCoordinator;
		private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;
		private readonly RelayServerContext _relayServerContext;

		/// <summary>
		/// Initializes a new instance of the <see cref="ConnectorHub{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
		/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator"/>.</param>
		/// <param name="tenantConnectorAdapterRegistry">The <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.</param>
		/// <param name="responseCoordinator">An <see cref="IResponseCoordinator{TResponse}"/>.</param>
		/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
		/// <param name="relayServerContext">An <see cref="RelayServerContext"/>.</param>
		public ConnectorHub(ILogger<ConnectorHub<TRequest, TResponse>> logger, IAcknowledgeCoordinator acknowledgeCoordinator,
			TenantConnectorAdapterRegistry<TRequest, TResponse> tenantConnectorAdapterRegistry,
			IResponseCoordinator<TResponse> responseCoordinator,
			IConnectionStatisticsWriter connectionStatisticsWriter,
			RelayServerContext relayServerContext)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_acknowledgeCoordinator = acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
			_tenantConnectorAdapterRegistry =
				tenantConnectorAdapterRegistry ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterRegistry));
			_responseCoordinator = responseCoordinator ?? throw new ArgumentNullException(nameof(responseCoordinator));
			_connectionStatisticsWriter = connectionStatisticsWriter ?? throw new ArgumentNullException(nameof(connectionStatisticsWriter));
			_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
		}

		/// <inheritdoc />
		public override async Task OnConnectedAsync()
		{
			var tenant = Context.User.GetTenantInfo();
			_logger.LogDebug("Connection incoming for tenant {@Tenant}", tenant);

			await _tenantConnectorAdapterRegistry.RegisterAsync(tenant.Id, Context.ConnectionId);
			await _connectionStatisticsWriter.SetConnectionTimeAsync(Context.ConnectionId, tenant.Id, _relayServerContext.OriginId,
				Context.GetHttpContext().Connection.RemoteIpAddress);

			await base.OnConnectedAsync();
		}

		/// <inheritdoc />
		public override async Task OnDisconnectedAsync(Exception exception)
		{
			_logger.LogDebug("Connection disconnected for tenant {@Tenant}", Context.User.GetTenantInfo());

			await _tenantConnectorAdapterRegistry.UnregisterAsync(Context.ConnectionId);
			await _connectionStatisticsWriter.SetDisconnectTimeAsync(Context.ConnectionId);

			await base.OnDisconnectedAsync(exception);
		}

		/// <inheritdoc />
		int? IConnectorTransport<TResponse>.BinarySizeThreshold { get; } = 64 * 1024; // 64kb

		/// <summary>
		/// Hub method.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		/// <seealso cref="IConnectorTransport{TResponse}.DeliverAsync"/>
		[HubMethodName("Deliver")]
		public async Task DeliverAsync(TResponse response)
		{
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
