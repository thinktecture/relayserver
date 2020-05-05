using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a dispatcher for client requests to a tenant.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantDispatcher<in TRequest>
		where TRequest : IRelayClientRequest
	{
		/// <summary>
		/// Dispatches the request to a tenant.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchRequestAsync(TRequest request);
	}
}
