using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Thinktecture.Relay.Server.Connector;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// A registry for <see cref="ITenantConnectorAdapter{TRequest}"/>s.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public class TenantConnectorAdapterRegistry<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		private readonly ILogger<TenantConnectorAdapterRegistry<TRequest, TResponse>> _logger;
		private readonly ITenantConnectorAdapterFactory<TRequest> _tenantConnectorAdapterFactory;
		private readonly ITenantHandlerFactory<TRequest, TResponse> _tenantHandlerFactory;

		private class TenantConnectorAdapterRegistration : IDisposable
		{
			private readonly ITenantConnectorAdapter<TRequest> _tenantConnectorAdapter;

			public ITenantHandler<TRequest> TenantHandler { get; }
			public Guid TenantId => _tenantConnectorAdapter.TenantId;

			public TenantConnectorAdapterRegistration(ITenantConnectorAdapter<TRequest> tenantConnectorAdapter,
				ITenantHandler<TRequest> tenantHandler)
			{
				_tenantConnectorAdapter = tenantConnectorAdapter ?? throw new ArgumentNullException(nameof(tenantConnectorAdapter));

				TenantHandler = tenantHandler ?? throw new ArgumentNullException(nameof(tenantHandler));
				TenantHandler.RequestReceived += OnRequestReceived;
			}

			private async Task OnRequestReceived(object sender, TRequest request) => await _tenantConnectorAdapter.RequestTargetAsync(request);

			public void Dispose()
			{
				TenantHandler.RequestReceived -= OnRequestReceived;
				(TenantHandler as IDisposable)?.Dispose();

				// ReSharper disable once SuspiciousTypeConversion.Global
				(_tenantConnectorAdapter as IDisposable)?.Dispose();
			}
		}

		private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>> _tenants
			= new ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>>();

		private readonly ConcurrentDictionary<string, TenantConnectorAdapterRegistration> _registrations
			= new ConcurrentDictionary<string, TenantConnectorAdapterRegistration>();

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategory}"/>.</param>
		/// <param name="tenantConnectorAdapterFactory">An <see cref="ITenantConnectorAdapterFactory{TRequest}"/>.</param>
		/// <param name="tenantHandlerFactory">An <see cref="ITenantHandlerFactory{TRequest,TResponse}"/>.</param>
		public TenantConnectorAdapterRegistry(ILogger<TenantConnectorAdapterRegistry<TRequest, TResponse>> logger,
			ITenantConnectorAdapterFactory<TRequest> tenantConnectorAdapterFactory,
			ITenantHandlerFactory<TRequest, TResponse> tenantHandlerFactory)
		{
			_logger = logger ?? throw new ArgumentNullException(nameof(logger));
			_tenantConnectorAdapterFactory =
				tenantConnectorAdapterFactory ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterFactory));
			_tenantHandlerFactory = tenantHandlerFactory ?? throw new ArgumentNullException(nameof(tenantHandlerFactory));
		}

		/// <summary>
		/// Registers the connection by creating an <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task RegisterAsync(Guid tenantId, string connectionId)
		{
			_logger.LogDebug("Registering connection {ConnectionId} for tenant {TenantId}", connectionId, tenantId);

			var tenantHandler = _tenantHandlerFactory.Create(tenantId, connectionId);
			var tenantConnectorAdapter = _tenantConnectorAdapterFactory.Create(tenantId, connectionId);

			var adapters = _tenants.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>());
			adapters[connectionId] = tenantConnectorAdapter;

			_registrations[connectionId] = new TenantConnectorAdapterRegistration(tenantConnectorAdapter, tenantHandler);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Unregisters the connection by destroying the corresponding <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task UnregisterAsync(string connectionId)
		{
			if (_registrations.TryRemove(connectionId, out var registration) && _tenants.TryGetValue(registration.TenantId, out var adapters))
			{
				_logger.LogDebug("Unregistering connection {ConnectionId} for tenant {TenantId}", connectionId, registration.TenantId);
				adapters.TryRemove(connectionId, out _);
			}
			else
			{
				_logger.LogWarning("Could not unregister connection {ConnectionId}", connectionId);
			}

			registration?.Dispose();

			return Task.CompletedTask;
		}

		/// <summary>
		/// Acknowledges
		/// </summary>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="acknowledgeId">The id to acknowledge.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task AcknowledgeRequestAsync(string connectionId, string acknowledgeId)
		{
			return _registrations.TryGetValue(connectionId, out var tenantConnectorAdapterRegistration)
				? tenantConnectorAdapterRegistration.TenantHandler.AcknowledgeAsync(acknowledgeId)
				: Task.CompletedTask;
		}
	}
}
