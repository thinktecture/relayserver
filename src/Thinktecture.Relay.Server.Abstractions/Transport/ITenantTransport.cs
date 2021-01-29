using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Transport
{
	/// <summary>
	/// An implementation of a dispatcher for client requests to a tenant.
	/// </summary>
	/// <typeparam name="T">The type of request.</typeparam>
	public interface ITenantTransport<in T>
		where T : IClientRequest
	{
		/// <summary>
		/// The maximum size of binary data the protocol is capable to serialize inline, or null if there is no limit.
		/// </summary>
		int? BinarySizeThreshold { get; }

		/// <summary>
		/// Transports the request to a tenant.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task TransportAsync(T request);
	}
}
