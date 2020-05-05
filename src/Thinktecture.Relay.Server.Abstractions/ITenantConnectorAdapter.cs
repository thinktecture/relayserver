using System;
using System.Threading.Tasks;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server
{
	/// <summary>
	/// An implementation of an adapter between a tenant and a connector. This consumes the messages from the underlying transport. The
	/// transport is responsible for distributing the requests between multiple connectors for one tenant.
	/// </summary>
	/// <typeparam name="TRequest">The type of request.</typeparam>
	/// <typeparam name="TResponse">The type of response.</typeparam>
	public interface ITenantConnectorAdapter<TRequest, TResponse>
		where TRequest : IRelayClientRequest
		where TResponse : IRelayTargetResponse
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
		/// Acknowledges a request.
		/// </summary>
		/// <param name="acknowledgeId">The unique id of the message.</param>
		/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
		Task AcknowledgeRequestAsync(string acknowledgeId);
	}
}
