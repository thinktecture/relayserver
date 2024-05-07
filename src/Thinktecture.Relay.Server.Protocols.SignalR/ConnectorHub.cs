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

/// <inheritdoc />
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
	private readonly ILogger _logger;
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

		if (relayServerOptions is null) throw new ArgumentNullException(nameof(relayServerOptions));

		_relayServerOptions = relayServerOptions.Value;
	}

	/// <inheritdoc />
	public override async Task OnConnectedAsync()
	{
		var tenantName = Context.User.GetTenantName();
		if (tenantName == string.Empty)
		{
			Log.ErrorNoTenantName(_logger, Context.ConnectionId);
			Context.Abort();
			return;
		}

		var tenant = await _tenantService.LoadTenantWithConfigAsync(tenantName);
		if (tenant is null)
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

				Log.IncomingConnectionCreatedTenant(_logger, Context.ConnectionId, tenantName);
			}
			else
			{
				Log.RejectingUnknownTenant(_logger, Context.ConnectionId, tenantName);
				Context.Abort();
				return;
			}
		}
		else if (_relayServerOptions.EnableAutomaticTenantCreation)
		{
			var displayName = Context.User.GetTenantDisplayName();
			var description = Context.User.GetTenantDescription();

			var needsUpdate = false;

			if (displayName is not null && tenant.DisplayName != displayName)
			{
				tenant.DisplayName = displayName;
				needsUpdate = true;
			}

			if (description is not null && tenant.Description != description)
			{
				tenant.Description = description;
				needsUpdate = true;
			}

			if (needsUpdate)
			{
				Log.IncomingConnectionUpdatedTenant(_logger, Context.ConnectionId, tenantName);
				await _tenantService.UpdateTenantAsync(tenantName, tenant);
			}
		}

		Log.IncomingConnection(_logger, Context.ConnectionId, tenant);

		await _connectorRegistry.RegisterAsync(Context.ConnectionId, tenant.Name, tenant.MaximumConcurrentConnectorRequests,
			Context.GetHttpContext()?.Connection.RemoteIpAddress);

		if (tenant.Config is not null)
		{
			await Clients.Caller.Configure(tenant.Config);
		}

		await base.OnConnectedAsync();
	}

	/// <inheritdoc />
	public override async Task OnDisconnectedAsync(Exception? exception)
	{
		if (exception is not null)
		{
			Log.DisconnectedError(_logger, exception, Context.ConnectionId, Context.User.GetTenantName());
		}
		else
		{
			Log.Disconnected(_logger, Context.ConnectionId, Context.User.GetTenantName());
		}

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
		Log.ReceivedResponse(_logger, Context.ConnectionId, response.RequestId);
		response.ConnectionId = Context.ConnectionId;

		await _responseDispatcher.DispatchAsync(response);
		await _connectionStatisticsWriter.UpdateLastSeenTimeAsync(Context.ConnectionId);
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
		Log.ReceivedAcknowledge(_logger, Context.ConnectionId, request.RequestId);
		await _acknowledgeCoordinator.ProcessAcknowledgeAsync(request);
		await _connectionStatisticsWriter.UpdateLastSeenTimeAsync(Context.ConnectionId);
	}
}
