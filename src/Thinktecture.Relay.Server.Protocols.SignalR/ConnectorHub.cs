using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Thinktecture.Relay.Acknowledgement;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Server.Persistence.Models;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Protocols.SignalR;

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

/// <inheritdoc/>
[Authorize(Constants.DefaultAuthenticationPolicy)]
// ReSharper disable once ClassNeverInstantiated.Global
public partial class ConnectorHub<TRequest, TResponse, TAcknowledge> : Hub<IConnector<TRequest>>
	where TRequest : IClientRequest
	where TResponse : ITargetResponse
	where TAcknowledge : IAcknowledgeRequest
{
	private readonly IAcknowledgeCoordinator<TAcknowledge> _acknowledgeCoordinator;
	private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;
	private readonly ConnectorRegistry<TRequest> _connectorRegistry;
	private readonly ILogger<ConnectorHub<TRequest, TResponse, TAcknowledge>> _logger;
	private readonly IResponseDispatcher<TResponse> _responseDispatcher;
	private readonly ITenantService _tenantService;
	private readonly RelayServerOptions _relayServerOptions;

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectorHub{TRequest,TResponse,TAcknowledge}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
	/// <param name="tenantService">An <see cref="ITenantService"/>.</param>
	/// <param name="relayServerOptions">An <see cref="IOptions{TOptions}"/>.</param>
	/// <param name="responseDispatcher">An <see cref="IResponseDispatcher{T}"/>.</param>
	/// <param name="acknowledgeCoordinator">An <see cref="IAcknowledgeCoordinator{T}"/>.</param>
	/// <param name="connectorRegistry">The <see cref="ConnectorRegistry{T}"/>.</param>
	/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
	public ConnectorHub(ILogger<ConnectorHub<TRequest, TResponse, TAcknowledge>> logger,
		ITenantService tenantService, IOptions<RelayServerOptions> relayServerOptions,
		IResponseDispatcher<TResponse> responseDispatcher, IAcknowledgeCoordinator<TAcknowledge> acknowledgeCoordinator,
		ConnectorRegistry<TRequest> connectorRegistry, IConnectionStatisticsWriter connectionStatisticsWriter)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_tenantService = tenantService ?? throw new ArgumentNullException(nameof(tenantService));
		_responseDispatcher = responseDispatcher ?? throw new ArgumentNullException(nameof(responseDispatcher));
		_acknowledgeCoordinator =
			acknowledgeCoordinator ?? throw new ArgumentNullException(nameof(acknowledgeCoordinator));
		_connectorRegistry = connectorRegistry ?? throw new ArgumentNullException(nameof(connectorRegistry));
		_connectionStatisticsWriter = connectionStatisticsWriter ??
			throw new ArgumentNullException(nameof(connectionStatisticsWriter));

		if (relayServerOptions == null) throw new ArgumentNullException(nameof(relayServerOptions));

		_relayServerOptions = relayServerOptions.Value;
	}

	/// <inheritdoc/>
	public override async Task OnConnectedAsync()
	{
		var tenantName = Context.User.GetTenantName();
		if (tenantName == string.Empty)
		{
			_logger.LogError(26100,
				"Rejecting incoming connection {TransportConnectionId} because of missing tenant name",
				Context.ConnectionId);
			Context.Abort();
			return;
		}

		var tenant = await _tenantService.LoadTenantWithConfigByNameAsync(tenantName);
		if (tenant == null)
		{
			if (_relayServerOptions.EnableAutomaticTenantCreation)
			{
				tenant = new Tenant()
				{
					Name = tenantName,
					DisplayName = Context.User.GetTenantDisplayName(),
					Description = Context.User.GetTenantDescription(),
				};

				await _tenantService.CreateTenantAsync(tenant);

				_logger.LogInformation(26107,
					"Incoming connection {TransportConnectionId} created tenant {TenantName}",
					Context.ConnectionId, tenantName);
			}
			else
			{
				_logger.LogError(26106,
					"Rejecting incoming connection {TransportConnectionId} because of unknown tenant {TenantName}",
					Context.ConnectionId, tenantName);
				Context.Abort();
				return;
			}
		}

		_logger.LogDebug(26101, "Incoming connection {TransportConnectionId} for tenant {@Tenant}",
			Context.ConnectionId, tenant);

		await _connectorRegistry.RegisterAsync(Context.ConnectionId, tenant.Id,
			Context.GetHttpContext()?.Connection.RemoteIpAddress);

		if (tenant.Config != null)
		{
			await Clients.Caller.Configure(tenant.Config);
		}

		await base.OnConnectedAsync();
	}

	/// <inheritdoc/>
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		if (exception == null)
		{
			_logger.LogWarning(26102, exception,
				"Connection {TransportConnectionId} disconnected for tenant {TenantName}", Context.ConnectionId,
				Context.User.GetTenantName());
		}
		else
		{
			_logger.LogDebug(26103, "Connection {TransportConnectionId} disconnected for tenant {TenantName}",
				Context.ConnectionId, Context.User.GetTenantName());
		}

		await _connectorRegistry.UnregisterAsync(Context.ConnectionId);
		await base.OnDisconnectedAsync(exception);
	}

	[LoggerMessage(26104, LogLevel.Debug,
		"Connection {TransportConnectionId} received response for request {RelayRequestId}")]
	partial void LogReceivedResponse(string transportConnectionId, Guid relayRequestId);

	/// <summary>
	/// Hub method.
	/// </summary>
	/// <param name="response">The target response.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HubMethodName("Deliver")]
	// ReSharper disable once UnusedMember.Global
	public async Task DeliverAsync(TResponse response)
	{
		LogReceivedResponse(Context.ConnectionId, response.RequestId);
		response.ConnectionId = Context.ConnectionId;

		await _responseDispatcher.DispatchAsync(response);
		await _connectionStatisticsWriter.UpdateLastSeenTimeAsync(Context.ConnectionId);
	}

	[LoggerMessage(26105, LogLevel.Debug,
		"Connection {TransportConnectionId} received acknowledgement for request {RelayRequestId}")]
	partial void LogReceivedAcknowledge(string transportConnectionId, Guid relayRequestId);

	/// <summary>
	/// Hub method.
	/// </summary>
	/// <param name="request">The acknowledge request.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HubMethodName("Acknowledge")]
	// ReSharper disable once UnusedMember.Global
	public async Task AcknowledgeAsync(TAcknowledge request)
	{
		LogReceivedAcknowledge(Context.ConnectionId, request.RequestId);
		await _acknowledgeCoordinator.ProcessAcknowledgeAsync(request);
		await _connectionStatisticsWriter.UpdateLastSeenTimeAsync(Context.ConnectionId);
	}

	/// <summary>
	/// Hub method.
	/// </summary>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	[HubMethodName("Pong")]
	// ReSharper disable once UnusedMember.Global
	public Task PongAsync()
		=> throw new NotImplementedException();
}
