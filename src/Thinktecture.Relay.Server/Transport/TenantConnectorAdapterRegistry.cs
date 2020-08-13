using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
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
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly ITenantConnectorAdapterFactory<TRequest> _tenantConnectorAdapterFactory;
		private readonly ITenantHandlerFactory<TRequest, TResponse> _tenantHandlerFactory;

		private class TenantConnectorAdapterRegistration : IDisposable
		{
			private readonly ITenantHandler<TRequest> _tenantHandler;
			private readonly ITenantConnectorAdapter<TRequest> _tenantConnectorAdapter;

			public Guid TenantId => _tenantConnectorAdapter.TenantId;

			public TenantConnectorAdapterRegistration(ITenantHandler<TRequest> tenantHandler,
				ITenantConnectorAdapter<TRequest> tenantConnectorAdapter)
			{
				_tenantHandler = tenantHandler ?? throw new ArgumentNullException(nameof(tenantHandler));
				_tenantConnectorAdapter = tenantConnectorAdapter ?? throw new ArgumentNullException(nameof(tenantConnectorAdapter));

				tenantHandler.RequestReceived += OnRequestReceived;
			}

			private async Task OnRequestReceived(object sender, TRequest @event) => await _tenantConnectorAdapter.RequestTargetAsync(@event);

			public void Dispose()
			{
				_tenantHandler.RequestReceived -= OnRequestReceived;
				(_tenantHandler as IDisposable)?.Dispose();

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
		/// <param name="tenantConnectorAdapterFactory">An <see cref="ITenantConnectorAdapterFactory{TRequest}"/>.</param>
		/// <param name="tenantHandlerFactory">An <see cref="ITenantHandlerFactory{TRequest,TResponse}"/>.</param>
		public TenantConnectorAdapterRegistry(ITenantConnectorAdapterFactory<TRequest> tenantConnectorAdapterFactory,
			ITenantHandlerFactory<TRequest, TResponse> tenantHandlerFactory)
		{
			_tenantConnectorAdapterFactory
				= tenantConnectorAdapterFactory ?? throw new ArgumentNullException(nameof(tenantConnectorAdapterFactory));
			_tenantHandlerFactory = tenantHandlerFactory ?? throw new ArgumentNullException(nameof(tenantHandlerFactory));
		}

		/// <summary>
		/// Registers the connection by creating an <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task RegisterAsync(Guid tenantId, string connectionId)
		{
			var tenantHandler = _tenantHandlerFactory.Create(tenantId);
			var tenantConnectorAdapter = _tenantConnectorAdapterFactory.Create(tenantId, connectionId);

			var adapters = _tenants.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest>>());
			adapters[connectionId] = tenantConnectorAdapter;

			_registrations[connectionId] = new TenantConnectorAdapterRegistration(tenantHandler, tenantConnectorAdapter);

			return Task.CompletedTask;
		}

		/// <summary>
		/// Unregisters the connection by destroying the corresponding <see cref="ITenantConnectorAdapter{TRequest}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		public Task UnregisterAsync(string connectionId)
		{
			if (_registrations.TryRemove(connectionId, out var registration) && _tenants.TryGetValue(registration.TenantId, out var adapters))
			{
				adapters.TryRemove(connectionId, out _);
			}

			registration?.Dispose();

			return Task.CompletedTask;
		}
	}
}
