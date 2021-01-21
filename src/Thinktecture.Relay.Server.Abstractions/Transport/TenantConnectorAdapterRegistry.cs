using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
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
		private readonly ITenantHandlerFactory<TRequest> _tenantHandlerFactory;

		private class TenantConnectorAdapterRegistration : IAsyncDisposable
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

			public async ValueTask DisposeAsync()
			{
				TenantHandler.RequestReceived -= OnRequestReceived;

				// ReSharper disable once SuspiciousTypeConversion.Global
				(TenantHandler as IDisposable)?.Dispose();

				// ReSharper disable once SuspiciousTypeConversion.Global
				if (TenantHandler is IAsyncDisposable tenantHandler)
				{
					await tenantHandler.DisposeAsync();
				}

				// ReSharper disable once SuspiciousTypeConversion.Global
				(_tenantConnectorAdapter as IDisposable)?.Dispose();

				// ReSharper disable once SuspiciousTypeConversion.Global
				if (_tenantConnectorAdapter is IAsyncDisposable tenantConnectorAdapter)
				{
					await tenantConnectorAdapter.DisposeAsync();
				}
			}
		}

		private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>> _tenants =
			new ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>>();

		private readonly ConcurrentDictionary<string, TenantConnectorAdapterRegistration> _registrations =
			new ConcurrentDictionary<string, TenantConnectorAdapterRegistration>();

		/// <summary>
		/// Initializes a new instance of the <see cref="TenantConnectorAdapterRegistry{TRequest,TResponse}"/> class.
		/// </summary>
		/// <param name="logger">An <see cref="ILogger{TCategory}"/>.</param>
		/// <param name="tenantConnectorAdapterFactory">An <see cref="ITenantConnectorAdapterFactory{TRequest}"/>.</param>
		/// <param name="tenantHandlerFactory">An <see cref="ITenantHandlerFactory{TRequest}"/>.</param>
		public TenantConnectorAdapterRegistry(ILogger<TenantConnectorAdapterRegistry<TRequest, TResponse>> logger,
			ITenantConnectorAdapterFactory<TRequest> tenantConnectorAdapterFactory,
			ITenantHandlerFactory<TRequest> tenantHandlerFactory)
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
		/// <remarks>This method does not fail if the connection was not registered.</remarks>
		public async Task UnregisterAsync(string connectionId)
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

			if (registration != null)
			{
				await registration.DisposeAsync();
			}
		}

		/// <summary>
		/// Acknowledges an <see cref="IClientRequest"/>.
		/// </summary>
		/// <param name="connectionId">The unique id of the connection.</param>
		/// <param name="acknowledgeId">The id to acknowledge.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task AcknowledgeRequestAsync(string connectionId, string acknowledgeId)
		{
			if (_registrations.TryGetValue(connectionId, out var tenantConnectorAdapterRegistration))
			{
				_logger.LogDebug("Acknowledging {AcknowledgeId} on connection {ConnectionId}", acknowledgeId, connectionId);
				return tenantConnectorAdapterRegistration.TenantHandler.AcknowledgeAsync(acknowledgeId);
			}

			_logger.LogWarning("Unknown connection {ConnectionId} to acknowledge {AcknowledgeId} received", connectionId, acknowledgeId);
			return Task.CompletedTask;
		}

		/// <summary>
		/// Tries to deliver the request to a random connected <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the result.</returns>
		public async Task<bool> TryDeliverRequestAsync(TRequest request, CancellationToken cancellationToken = default)
		{
			if (!_tenants.TryGetValue(request.TenantId, out var adapters))
			{
				return false;
			}

			var snapshot = adapters.Values.ToArray();
			if (snapshot.Length == 0)
			{
				return false;
			}

			var tenantConnectorAdapter = snapshot[new Random().Next(adapters.Count)];
			_logger.LogDebug("Delivering request {RequestId} to local connection {ConnectionId}", request.RequestId,
				tenantConnectorAdapter.ConnectionId);
			await tenantConnectorAdapter.RequestTargetAsync(request, cancellationToken);

			return true;
		}
	}
}
