using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Abstractions;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of a dispatcher for client requests to a connector and target responses from connectors.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	/// <remarks>The implementing instance should be a singleton.</remarks>
	public interface ITenantDispatcher<in TRequest, TResponse>
		where TRequest : ITransportClientRequest
		where TResponse : ITransportTargetResponse
	{
		/// <summary>
		/// Dispatches the request to a tenant.
		/// </summary>
		/// <param name="tenantId">The unique id of the tenant.</param>
		/// <param name="request">The client request.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation, which wraps the response.</returns>
		Task<TResponse> DispatchRequestAsync(Guid tenantId, TRequest request);

		/// <summary>
		/// Dispatches the response to the <see cref="TaskCompletionSource{TResult}"/> related to the request id.
		/// </summary>
		/// <param name="response">The target response.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task DispatchResponseAsync(TResponse response);
	}
}
