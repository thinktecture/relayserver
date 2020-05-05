using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	// TODO change name of this?
	/// <summary>
	/// An implementation of a locator for an <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/>. This mangles the communication
	/// between the underlying tenant transport and connector transport.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface ITenantConnectorAdapterLocator<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
	{
		/// <summary>
		/// Registers the connection by creating an <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RegisterAdapterAsync(Guid tenantId, string connectionId);

		/// <summary>
		/// Unregisters the connection by destroying the corresponding <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task UnregisterAdapterAsync(string connectionId);

		/// <summary>
		/// Gets the <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/> for the provided connection id.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the adapter or null if the connection id is
		/// unknown.</returns>
		Task<ITenantConnectorAdapter<TRequest, TResponse>> GetTenantConnectorAdapterAsync(string connectionId);
	}
}
