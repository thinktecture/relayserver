using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Thinktecture.Relay.Server.Factories;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <inheritdoc />
	public class TenantConnectorAdapterLocator<TRequest, TResponse> : ITenantConnectorAdapterLocator<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		private readonly TenantConnectorAdapterFactory<TRequest, TResponse> _tenantConnectorAdapterFactory;

		private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest, TResponse>>> _tenants =
			new ConcurrentDictionary<Guid, ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest, TResponse>>>();

		private readonly ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest, TResponse>> _connections
			= new ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest, TResponse>>();

		/// <summary>
		/// Initializes a new instance of <see cref="TenantConnectorAdapterFactory{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantConnectorAdapterFactory">The <see cref="TenantConnectorAdapterFactory{TRequest,TResponse}"/>.</param>
		public TenantConnectorAdapterLocator(TenantConnectorAdapterFactory<TRequest, TResponse> tenantConnectorAdapterFactory)
			=> _tenantConnectorAdapterFactory = tenantConnectorAdapterFactory;

		/// <inheritdoc />
		public Task RegisterAdapterAsync(Guid tenantId, string connectionId)
		{
			var adapter = _tenantConnectorAdapterFactory.Create(tenantId, connectionId);

			var adapters = _tenants.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, ITenantConnectorAdapter<TRequest, TResponse>>());
			adapters[connectionId] = adapter;

			_connections[connectionId] = adapter;

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task UnregisterAdapterAsync(string connectionId)
		{
			if (_connections.TryRemove(connectionId, out var adapter) && _tenants.TryGetValue(adapter.TenantId, out var adapters))
			{
				adapters.TryRemove(connectionId, out _);
			}

			return Task.CompletedTask;
		}

		/// <inheritdoc />
		public Task<ITenantConnectorAdapter<TRequest, TResponse>> GetTenantConnectorAdapterAsync(string connectionId)
		{
			_connections.TryGetValue(connectionId, out var adapter);
			return Task.FromResult(adapter);
		}
	}
}
