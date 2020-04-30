using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

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
		where TRequest : ITransportClientRequest
		where TResponse : ITransportTargetResponse
	{
		/// <summary>
		/// Registers the connection by creating a <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RegisterAdapterAsync(string connectionId);

		/// <summary>
		/// Unregisters the connection by destroying the corresponding <see cref="ITenantConnectorAdapter{TRequest,TResponse}"/>.
		/// </summary>
		/// <param name="connectionId">The unique id for the connection.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task UnregisterAdapterAsync(string connectionId);
	}
}
