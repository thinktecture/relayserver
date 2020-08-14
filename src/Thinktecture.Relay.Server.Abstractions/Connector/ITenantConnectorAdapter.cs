using System;
using System.Threading;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Connector
{
	/// <summary>
	/// An implementation of an adapter between a tenant and a connector. This consumes the messages from the underlying transport. The
	/// transport is responsible for distributing the requests between multiple connectors for one tenant.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	public interface ITenantConnectorAdapter<in TRequest>
		where TRequest : IClientRequest
	{
		/// <summary>
		/// The unique id of the tenant.
		/// </summary>
		Guid TenantId { get; }

		/// <summary>
		/// The unique id of the connection.
		/// </summary>
		string ConnectionId { get; }

		/// <summary>
		/// Request the response from a target.
		/// </summary>
		/// <param name="request">The client request.</param>
		/// <param name="cancellationToken">The token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task RequestTargetAsync(TRequest request, CancellationToken cancellationToken = default);
	}
}
