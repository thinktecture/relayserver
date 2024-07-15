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
	private readonly ILogger _logger;

	private readonly ConcurrentDictionary<string, ConnectorRegistration> _registrations =
		new ConcurrentDictionary<string, ConnectorRegistration>();

	private readonly RelayServerContext _relayServerContext;
	private readonly IServiceProvider _serviceProvider;

	private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, IConnectorTransport<T>>> _tenants =
		new ConcurrentDictionary<string, ConcurrentDictionary<string, IConnectorTransport<T>>>();

	/// <summary>
	/// Initializes a new instance of the <see cref="ConnectorRegistry{T}"/> class.
	/// </summary>
	/// <param name="logger">An <see cref="ILogger{TCategoryName}"/>.</param>
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
	/// <param name="maximumConcurrentRequests">The amount of maximum concurrent requests.</param>
	/// <param name="remoteIpAddress">The optional remote ip address of the connection.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public async Task RegisterAsync(string connectionId, string tenantName, int maximumConcurrentRequests,
		IPAddress? remoteIpAddress = null)
	{
		Log.RegisteringConnection(_logger, connectionId, tenantName);

		var registration =
			ActivatorUtilities.CreateInstance<ConnectorRegistration>(_serviceProvider, tenantName, connectionId,
				maximumConcurrentRequests);

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
			Log.UnregisteringConnection(_logger, connectionId, registration.TenantName);
			connectors.TryRemove(connectionId, out _);
		}
		else
		{
			Log.CouldNotUnregisterConnection(_logger, connectionId);
		}

		await _connectionStatisticsWriter.SetDisconnectTimeAsync(connectionId);
		registration?.Dispose();
	}

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

		Log.UnknownRequestConnection(_logger, connectionId, request.RequestId);

		return Task.CompletedTask;
	}

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

		Log.UnknownAcknowledgeConnection(_logger, connectionId, acknowledgeId);
		return Task.CompletedTask;
	}

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

		Log.DeliveringRequest(_logger, request.RequestId, kvp.Key);
		await kvp.Value.TransportAsync(request, cancellationToken);

		return true;
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	private class ConnectorRegistration : IDisposable
	{
		public string TenantName { get; }

		public IConnectorTransport<T> ConnectorTransport { get; }

		public ITenantHandler TenantHandler { get; }

		public ConnectorRegistration(string tenantName, string connectionId, int maximumConcurrentRequests,
			IConnectorTransportFactory<T> connectorTransportFactory, ITenantHandlerFactory tenantHandlerFactory)
		{
			if (connectionId is null) throw new ArgumentNullException(nameof(connectionId));
			if (connectorTransportFactory is null) throw new ArgumentNullException(nameof(connectorTransportFactory));
			if (tenantHandlerFactory is null) throw new ArgumentNullException(nameof(tenantHandlerFactory));

			TenantName = tenantName;
			ConnectorTransport = connectorTransportFactory.Create(connectionId);
			TenantHandler = tenantHandlerFactory.Create(tenantName, connectionId, maximumConcurrentRequests);
		}

		public void Dispose()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			(ConnectorTransport as IDisposable)?.Dispose();
			(TenantHandler as IDisposable)?.Dispose();
		}
	}
}
