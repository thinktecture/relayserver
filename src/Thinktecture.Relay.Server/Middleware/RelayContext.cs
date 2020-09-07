using System;
using Thinktecture.Relay.Server.Transport;
using Thinktecture.Relay.Transport;

namespace Thinktecture.Relay.Server.Middleware
{
	/// <inheritdoc />
	public class RelayContext<TRequest, TResponse> : IRelayContext<TRequest, TResponse>
		where TRequest : IClientRequest
		where TResponse : ITargetResponse
	{
		/// <inheritdoc />
		public Guid RequestId => ClientRequest.RequestId;

		/// <inheritdoc />
		public TRequest ClientRequest { get; set; }

		/// <inheritdoc />
		public TResponse TargetResponse { get; set; }

		/// <inheritdoc />
		public bool IsConnectorAvailable { get; }

		/// <inheritdoc />
		public bool ForceConnectorDelivery { get; set; }

		/// <inheritdoc />
		public IAsyncDisposable ResponseDisposable { get; set; }
	}
}
