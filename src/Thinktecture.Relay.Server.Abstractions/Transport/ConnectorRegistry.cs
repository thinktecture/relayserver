using System;
using System.Collections.Concurrent;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Persistence;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport;

/// <summary>
/// A registry for connectors.
/// </summary>
/// <typeparam name="T">The type of request.</typeparam>
public partial class ConnectorRegistry<T>
	where T : IClientRequest
{
	private readonly IConnectionStatisticsWriter _connectionStatisticsWriter;
	private readonly ILogger<ConnectorRegistry<T>> _logger;

	private readonly ConcurrentDictionary<string, ConnectorRegistration> _registrations =
		new ConcurrentDictionary<string, ConnectorRegistration>();

	private readonly RelayServerContext _relayServerContext;
	private readonly IServiceProvider _serviceProvider;

	private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IConnectorTransport<T>>> _tenants =
		new ConcurrentDictionary<string, ConcurrentDictionary<string, IConnectorTransport<T>>>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectorRegistry{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategory}"/>.</param>
	/// <param name="serviceProvider">An <see cref="IServiceProvider"/></param>
	/// <param name="connectionStatisticsWriter">An <see cref="IConnectionStatisticsWriter"/>.</param>
	/// <param name="relayServerContext">The <see cref="RelayServerContext"/>.</param>
	public ConnectorRegistry(ILogger<ConnectorRegistry<T>> logger, IServiceProvider serviceProvider,
		IConnectionStatisticsWriter connectionStatisticsWriter, RelayServerContext relayServerContext)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
		_connectionStatisticsWriter = connectionStatisticsWriter ??
			throw new ArgumentNullException(nameof(connectionStatisticsWriter));
		_relayServerContext = relayServerContext ?? throw new ArgumentNullException(nameof(relayServerContext));
	}

	/// <summary>
	/// Registers the connection.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="tenantName">The unique name of the tenant.</param>
	/// <param name="remoteIpAddress">The optional remote ip address of the connection.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task RegisterAsync(string connectionId, string tenantName, IPAddress? remoteIpAddress = null)
	{
		_logger.LogDebug(22100,
			"Registering connection {TransportConnectionId} for tenant {TenantName}",
			connectionId, tenantName);

		var registration =
			ActivatorUtilities.CreateInstance<ConnectorRegistration>(_serviceProvider, tenantName, connectionId);

		var transports = _tenants.GetOrAdd(tenantName, _ => new ConcurrentDictionary<string, IConnectorTransport<T>>());
		transports[connectionId] = registration.ConnectorTransport;

		_registrations[connectionId] = registration;

		await _connectionStatisticsWriter.SetConnectionTimeAsync(connectionId, tenantName, _relayServerContext.OriginId,
			remoteIpAddress);
	}

	/// <summary>
	/// Unregisters the connection.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	/// <remarks>This method does not fail if the connection was not registered.</remarks>
	public async Task UnregisterAsync(string connectionId)
	{
		if (_registrations.TryRemove(connectionId, out var registration) &&
		    _tenants.TryGetValue(registration.TenantName, out var connectors))
		{
			_logger.LogDebug(22101, "Unregistering connection {TransportConnectionId} for tenant {TenantName}",
				connectionId, registration.TenantName);
			connectors.TryRemove(connectionId, out _);
		}
		else
		{
			_logger.LogWarning(22102, "Could not unregister connection {TransportConnectionId}", connectionId);
		}

		await _connectionStatisticsWriter.SetDisconnectTimeAsync(connectionId);

		registration?.Dispose();
	}

	[LoggerMessage(22103, LogLevel.Warning, "Unknown connection {TransportConnectionId} to transport request {RelayRequestId} to")]
	partial void LogUnknownRequestConnection(string transportConnectionId, Guid relayRequestId);

	/// <summary>
	/// Transports a client request.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="request">The client request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task TransportRequestAsync(string connectionId, T request, CancellationToken cancellationToken = default)
	{
		if (_registrations.TryGetValue(connectionId, out var registration))
			return registration.ConnectorTransport.TransportAsync(request, cancellationToken);

		LogUnknownRequestConnection(connectionId, request.RequestId);

		return Task.CompletedTask;
	}

	[LoggerMessage(22104, LogLevel.Warning,
		"Unknown connection {TransportConnectionId} to transport acknowledge {AcknowledgeId} to")]
	partial void LogUnknownAcknowledgeConnection(string transportConnectionId, string acknowledgeId);

	/// <summary>
	/// Acknowledges a client request.
	/// </summary>
	/// <param name="connectionId">The unique id of the connection.</param>
	/// <param name="acknowledgeId">The id to acknowledge.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public Task AcknowledgeRequestAsync(string connectionId, string acknowledgeId,
		CancellationToken cancellationToken = default)
	{
		if (_registrations.TryGetValue(connectionId, out var registration))
			return registration.TenantHandler.AcknowledgeAsync(acknowledgeId, cancellationToken);

		LogUnknownAcknowledgeConnection(connectionId, acknowledgeId);
		return Task.CompletedTask;
	}

	[LoggerMessage(22105, LogLevel.Trace, "Delivering request {RelayRequestId} to local connection {TransportConnectionId}")]
	partial void LogDeliveringRequest(Guid relayRequestId, string? transportConnectionId);

	/// <summary>
	/// Tries to deliver the client request to a random connector.
	/// </summary>
	/// <param name="request">The client request.</param>
	/// <param name="cancellationToken">
	/// The token to monitor for cancellation requests. The default value is
	/// <see cref="CancellationToken.None"/>.
	/// </param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the result.</returns>
	public async Task<bool> TryDeliverRequestAsync(T request, CancellationToken cancellationToken = default)
	{
		if (!_tenants.TryGetValue(request.TenantName, out var transports)) return false;

		var snapshot = transports.ToArray();
		if (snapshot.Length == 0) return false;

		var kvp = snapshot[new Random().Next(snapshot.Length)];

		LogDeliveringRequest(request.RequestId, kvp.Key);
		await kvp.Value.TransportAsync(request, cancellationToken);

		return true;
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class ConnectorRegistration : IDisposable
	{
		public string TenantName { get; }
		public IConnectorTransport<T> ConnectorTransport { get; }
		public ITenantHandler TenantHandler { get; }

		public ConnectorRegistration(string tenantName, string connectionId,
			IConnectorTransportFactory<T> connectorTransportFactory,
			ITenantHandlerFactory tenantHandlerFactory)
		{
			if (connectionId == null) throw new ArgumentNullException(nameof(connectionId));
			if (connectorTransportFactory == null) throw new ArgumentNullException(nameof(connectorTransportFactory));
			if (tenantHandlerFactory == null) throw new ArgumentNullException(nameof(tenantHandlerFactory));

			TenantName = tenantName;
			ConnectorTransport = connectorTransportFactory.Create(connectionId);
			TenantHandler = tenantHandlerFactory.Create(tenantName, connectionId);
		}

		public void Dispose()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			(ConnectorTransport as IDisposable)?.Dispose();
			(TenantHandler as IDisposable)?.Dispose();
		}
	}
}
